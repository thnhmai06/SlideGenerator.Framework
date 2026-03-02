# Image Module Documentation

## Overview

The Image module provides advanced image processing capabilities including:
- **Face Detection** with YuNet ONNX model
- **ROI (Region of Interest) Calculation** with multiple algorithms
- **Image Manipulation** utilities (resize, crop, etc.)
- **Provider Pattern** for dependency injection

## Architecture

### Provider Pattern

The face detection system uses a clean provider pattern:

```
IFaceDetectorModelProvider (interface)
    └── GetCurrentModelAsync()

FaceDetectorModelManager (implements IFaceDetectorModelProvider)
    ├── Manages current model lifecycle
    ├── Lazy initialization via factory
    └── Model switching support

IFaceDetectorModelFactory (interface)
    └── CreateModel(modelKey)

FaceDetectorModelFactory (implementation)
    └── Creates YuNetModel instances
```

### Dependency Injection Setup

```csharp
// In your DI container setup (e.g., Program.cs)
services.AddSingleton<IFaceDetectorModelFactory, FaceDetectorModelFactory>();
services.AddSingleton<FaceDetectorModelManager>();
services.AddSingleton<IFaceDetectorModelProvider>(sp => 
    sp.GetRequiredService<FaceDetectorModelManager>());

// Services inject IFaceDetectorModelProvider
public class MyService(IFaceDetectorModelProvider faceDetectorProvider)
{
    // Use provider.GetCurrentModelAsync() when needed
}
```

## Face Detection

### YuNet Model

**Features:**
- ONNX-based face detection
- 5 facial landmarks (right eye, left eye, nose, mouth corners)
- Confidence scoring
- Automatic preprocessing with coordinate unmapping

### Basic Usage

```csharp
using SlideGenerator.Framework.Features.Image.Contracts;
using SlideGenerator.Framework.Features.Image.Models.FaceDetection;

public class FaceDetectionService(IFaceDetectorModelProvider provider)
{
    public async Task<List<FaceInfo>> DetectFaces(Mat image)
    {
        // Get current model (auto-initialized)
        var model = await provider.GetCurrentModelAsync();
        
        // Detect faces
        var faces = await model.DetectAsync(image);
        
        // Filter by confidence
        return faces.Where(f => f.Confidence >= 0.7f).ToList();
    }
}
```

### Face Detection Output

```csharp
public record FaceInfo
{
    public Rectangle BoundingBox { get; }      // Face location
    public float Confidence { get; }           // Detection score (0-1)
    public Point? RightEye { get; }           // Right eye center
    public Point? LeftEye { get; }            // Left eye center
    public Point? Nose { get; }               // Nose tip
    public Point? MouthRight { get; }         // Right mouth corner
    public Point? MouthLeft { get; }          // Left mouth corner
}
```

### Preprocessing & Coordinate Mapping

YuNet internally handles:

1. **Resize + Pad to 320×320**:
   - Maintains aspect ratio
   - Adds black padding if needed
   - Example: 1920×1080 → 320×180 → 320×320 (padded)

2. **Detection**:
   - Runs on preprocessed 320×320 image
   - Returns coordinates in preprocessed space

3. **Coordinate Unmapping**:
   - Maps detection results back to original image coordinates
   - Automatic and transparent to caller

```csharp
// Example flow:
// Input: 1920×1080 image
// Internal preprocessing: resize to 320×180, pad to 320×320
// Detection: face at [50, 100, 80, 80] on 320×320 image
// Unmapping: face at [300, 540, 480, 480] on original 1920×1080 image
// Output: FaceInfo with coordinates in original space
```

### Model Management

```csharp
// Inject manager for control operations
public class ModelController(FaceDetectorModelManager manager)
{
    // Switch to different model
    public async Task SwitchModel(FaceDetectorModelKey newKey)
    {
        await manager.SelectModelAsync(newKey);
        // Old model disposed, new model created lazily
    }
    
    // Get current model key
    public FaceDetectorModelKey GetCurrentKey()
    {
        return manager.CurrentModelKey;
    }
}
```

## ROI Calculation

### ROI Types

#### 1. CenterRoi (Singleton)

Simple center cropping without any analysis.

```csharp
var roi = CenterRoi.Instance;
var cropRect = await roi.CalculateRoiAsync(mat, new Size(800, 600));
```

**Use Case:** Fast, predictable cropping

#### 2. ProminentRoi (Singleton)

Saliency-based cropping that focuses on visually prominent regions.

```csharp
var roi = ProminentRoi.Instance;
var cropRect = await roi.CalculateRoiAsync(mat, new Size(800, 600));
```

**Algorithm:**
1. Compute saliency map
2. Apply Gaussian blur
3. Find maximum saliency point
4. Center crop around that point

**Use Case:** Automatic focus on interesting areas

#### 3. RuleOfThirdsRoi (Instance, Requires Provider)

Face-aware cropping using rule of thirds composition.

```csharp
// Constructor requires provider
var roi = new RuleOfThirdsRoi(faceDetectorProvider);
var cropRect = await roi.CalculateRoiAsync(mat, new Size(800, 600));
```

**Algorithm:**
1. Detect all faces
2. Calculate average eye center position
3. Position eye center at rule of thirds intersection
4. Fallback to center (50%, 50%) if no faces

**Use Case:** Professional portrait cropping

### Using ROI Extension Method

```csharp
using SlideGenerator.Framework.Features.Image.Models.Roi;

// For RuleOfThirds (requires provider)
var calculator = await RoiType.RuleOfThirds.GetCalculator(faceDetectorProvider);

// For Center or Prominent (no provider needed)
var calculator = await RoiType.Center.GetCalculator();

// Calculate ROI
var cropRect = await calculator.CalculateRoiAsync(mat, targetSize);
```

**Note:** Calling `RoiType.RuleOfThirds.GetCalculator(null)` throws `ArgumentNullException`.

## Image Manipulation

### ManipulatingService

Static utility class for common image operations.

**Important:** All methods use `OpenCvSharp.Size`, not `System.Drawing.Size`.

#### Resize

```csharp
using OpenCvSharp;
using SlideGenerator.Framework.Features.Image.Services;
using ImageMagick;

// Load image using Framework's ConvertingService
using var magickImage = new MagickImage("photo.png");
var mat = ConvertingService.ConvertImageToMat(magickImage);
if (mat == null) return;

try
{
    var targetSize = new Size(1920, 1080); // OpenCvSharp.Size
    
    // In-place resize using Framework
    ManipulatingService.Resize(ref mat, targetSize, InterpolationFlags.Linear);
    
    // Convert back using Framework
    var resultBytes = ConvertingService.ConvertMatToImage(mat);
}
finally
{
    mat.Dispose();
}
```

**Interpolation Options:**
- `InterpolationFlags.Linear` - Bilinear interpolation (default)
- `InterpolationFlags.Area` - Resampling using pixel area relation
- `InterpolationFlags.Cubic` - Bicubic interpolation
- `InterpolationFlags.Lanczos4` - Lanczos interpolation over 8×8 neighborhood

#### Crop

```csharp
using System.Drawing;
using SlideGenerator.Framework.Features.Image.Services;
using ImageMagick;

// Load image using Framework
using var magickImage = new MagickImage("photo.png");
var mat = ConvertingService.ConvertImageToMat(magickImage);
if (mat == null) return;

try
{
    var cropRect = new Rectangle(100, 100, 800, 600);
    
    // In-place crop using Framework
    ManipulatingService.Crop(ref mat, cropRect);
    
    // Convert back using Framework
    var resultBytes = ConvertingService.ConvertMatToImage(mat);
}
finally
{
    mat.Dispose();
}
```

#### Clamp to Border

```csharp
// Clamp rectangle to stay within border
var border = new Rectangle(0, 0, 1920, 1080);
var rect = new Rectangle(1800, 1000, 400, 300); // Exceeds border

var clamped = ManipulatingService.ClampToBorder(rect, border);
// Result: Rectangle adjusted to fit within border

// Clamp point to border
var point = new Point(2000, 1200); // Outside border
var clampedPoint = ManipulatingService.ClampToBorder(point, border);
// Result: Point(1919, 1079) - at border edge
```

#### Get Max Aspect Size

```csharp
var originalSize = new Size(1920, 1080); // OpenCvSharp.Size
var targetSize = new Size(800, 800);

var maxSize = ManipulatingService.GetMaxAspectSize(originalSize, targetSize);
// Result: Size(800, 450) - maintains 16:9 aspect, fits in 800×800
```

## Complete Example

### Face-Aware Image Processing

```csharp
using OpenCvSharp;
using SlideGenerator.Framework.Features.Image.Contracts;
using SlideGenerator.Framework.Features.Image.Services;
using SlideGenerator.Framework.Features.Image.Models.Roi;
using ImageMagick;

public class ImageProcessor(IFaceDetectorModelProvider faceDetectorProvider)
{
    public async Task<byte[]> ProcessImage(
        string imagePath,
        Size targetSize,
        RoiType roiType)
    {
        // Load image using Framework's ConvertingService
        using var magickImage = new MagickImage(imagePath);
        var mat = ConvertingService.ConvertImageToMat(magickImage);
        if (mat == null || mat.Empty())
            return Array.Empty<byte>();
        
        try
        {
            // Get appropriate ROI calculator
            var calculator = roiType == RoiType.RuleOfThirds
                ? await roiType.GetCalculator(faceDetectorProvider)
                : await roiType.GetCalculator();
            
            // Calculate crop rectangle
            // (RuleOfThirds will use face detection internally)
            var cropRect = await calculator.CalculateRoiAsync(mat, targetSize);
            
            // Apply crop using Framework
            ManipulatingService.Crop(ref mat, cropRect);
            
            // Resize to target using Framework
            ManipulatingService.Resize(ref mat, targetSize);
            
            // Convert back to bytes using Framework
            return ConvertingService.ConvertMatToImage(mat);
        }
        finally
        {
            mat.Dispose();
        }
    }
}
```

## Performance Considerations

### Face Detection
- **Model initialization**: ~50-100ms (one-time, lazy)
- **320×320 inference**: ~10-30ms per image (CPU)
- **Preprocessing**: ~5-10ms
- **Total**: ~15-40ms per detection

### Memory Management
- Always dispose `Mat` objects: use `using` statements
- Model kept in memory after first use (~2MB)
- Per-detection buffers auto-released

### Optimization Tips
1. Reuse provider instances (singleton via DI)
2. Batch process when possible (model stays warm)
3. Use appropriate `InputSize` (320×320 is balanced)
4. Dispose `Mat` objects promptly

## Error Handling

```csharp
// Model initialization failure
try {
    var model = await provider.GetCurrentModelAsync();
} catch (InvalidOperationException ex) {
    // Model could not be initialized
    // Check: ONNX file exists, OpenCV DNN support
}

// RuleOfThirds without provider
try {
    var calc = await RoiType.RuleOfThirds.GetCalculator(null);
} catch (ArgumentNullException ex) {
    // "Face detector model provider is required"
}

// Empty/invalid image
var faces = await model.DetectAsync(emptyMat);
// Returns: empty list (no exception)
```

## Testing

### Mock Provider

```csharp
public class MockFaceDetectorProvider : IFaceDetectorModelProvider
{
    public FaceDetectorModel ModelToReturn { get; set; }
    
    public Task<FaceDetectorModel> GetCurrentModelAsync()
    {
        return Task.FromResult(ModelToReturn);
    }
}

// Use in tests
var mockProvider = new MockFaceDetectorProvider 
{ 
    ModelToReturn = new MockFaceDetectorModel() 
};

var roi = new RuleOfThirdsRoi(mockProvider);
// Test without real face detection
```

## API Reference

### Interfaces

```csharp
namespace SlideGenerator.Framework.Features.Image.Contracts;

public interface IFaceDetectorModelProvider
{
    Task<FaceDetectorModel> GetCurrentModelAsync();
}

public interface IFaceDetectorModelFactory
{
    FaceDetectorModel CreateModel(FaceDetectorModelKey modelKey);
}
```

### Key Classes

```csharp
namespace SlideGenerator.Framework.Features.Image.Services;

// Manager
public sealed class FaceDetectorModelManager : IFaceDetectorModelProvider
{
    public FaceDetectorModelKey CurrentModelKey { get; }
    public Task<FaceDetectorModel> GetCurrentModelAsync();
    public Task SelectModelAsync(FaceDetectorModelKey modelKey);
}

// Factory
public sealed class FaceDetectorModelFactory : IFaceDetectorModelFactory
{
    public FaceDetectorModel CreateModel(FaceDetectorModelKey modelKey);
}

// Manipulation utilities
public static class ManipulatingService
{
    public static void Resize(ref Mat mat, Size size, InterpolationFlags interpolation = InterpolationFlags.Area);
    public static void Crop(ref Mat mat, Rectangle rect);
    public static Rectangle ClampToBorder(Rectangle rect, Rectangle border);
    public static Point ClampToBorder(Point point, Rectangle border);
    public static Size GetMaxAspectSize(Size original, Size target);
}
```

### ROI Calculators

```csharp
namespace SlideGenerator.Framework.Features.Image.Entities.Roi;

// Singletons
public sealed class CenterRoi : RoiCalculator
{
    public static CenterRoi Instance { get; }
}

public sealed class ProminentRoi : RoiCalculator
{
    public static ProminentRoi Instance { get; }
}

// Instance (requires provider)
public sealed class RuleOfThirdsRoi : RoiCalculator
{
    public RuleOfThirdsRoi(IFaceDetectorModelProvider faceDetectorProvider);
}
```

## Migration from Old API

If you were using the old API directly:

**Old:**
```csharp
var model = new YuNetModel();
await model.InitAsync();
var faces = await model.DetectAsync(mat);
```

**New:**
```csharp
// Setup DI first
services.AddSingleton<IFaceDetectorModelFactory, FaceDetectorModelFactory>();
services.AddSingleton<FaceDetectorModelManager>();
services.AddSingleton<IFaceDetectorModelProvider>(sp => 
    sp.GetRequiredService<FaceDetectorModelManager>());

// Then inject provider
public MyClass(IFaceDetectorModelProvider provider)
{
    var model = await provider.GetCurrentModelAsync();
    var faces = await model.DetectAsync(mat);
}
```

**Benefits:**
- Lazy initialization
- Proper DI support
- Testable with mocks
- Centralized model lifecycle

