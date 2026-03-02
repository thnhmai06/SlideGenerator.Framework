# SlideGenerator.Framework

<p>
  <img src="https://img.shields.io/badge/.NET-10.0-512bd4?style=flat-square" alt=".NET 10" />
  <img src="https://img.shields.io/badge/OpenCvSharp4-4.13.0-orange?style=flat-square" alt="OpenCvSharp4" />
  <a href="https://www.codefactor.io/repository/github/thnhmai06/slidegenerator.framework"><img src="https://www.codefactor.io/repository/github/thnhmai06/slidegenerator.framework/badge" alt="CodeFactor" /></a>
  <a href="https://ghloc.vercel.app/thnhmai06/SlideGenerator.Framework"><img src="https://img.shields.io/endpoint?url=https://ghloc.vercel.app/api/thnhmai06/SlideGenerator.Framework/badge%3Ffilter=.ts$,.tsx$,.html$,.css$,.cs$%26format=human&style=flat-square&color=blue" alt="Lines of Code" /></a>
  <a href="https://github.com/thnhmai06/SlideGenerator.Framework/blob/main/LICENSE"><img src="https://img.shields.io/github/license/thnhmai06/SlideGenerator.Framework?style=flat-square" alt="License" /></a>
</p>

A powerful, standalone .NET library for orchestrating the generation of PowerPoint presentations from Excel data
sources. It provides high-level abstractions for template manipulation, data extraction, and intelligent image
processing.

**Documentation:** [English](Documents/en/overview.md) | [Tiếng Việt](Documents/vi/overview.md)

## Modules

The framework is composed of four core modules:

| Module        | Namespace                        | Description                                                                             |
|:--------------|:---------------------------------|:----------------------------------------------------------------------------------------|
| **☁️ Cloud**  | `SlideGenerator.Framework.Cloud` | Resolves direct download links from Google Drive, OneDrive, and Google Photos.          |
| **📊 Sheet**  | `SlideGenerator.Framework.Sheet` | efficient reading of Excel (.xlsx) and CSV files.                                       |
| **🖼️ Slide** | `SlideGenerator.Framework.Slide` | PowerPoint manipulation: template loading, slide cloning, text/image replacement.       |
| **🧠 Image**  | `SlideGenerator.Framework.Image` | Advanced image processing: Face detection, ROI (Region of Interest) cropping, resizing. |

## Prerequisites

### OpenCvSharp4 Runtime

This framework uses **OpenCvSharp4** for computer vision tasks. OpenCvSharp4 provides [native OpenCV bindings](https://github.com/shimat/opencvsharp/blob/main/README.md#native-bindings) for .NET
with platform-specific runtime packages. **Please install the appropriate runtime package for your target OS**.

| OS                             | Runtime Package                                       | Description                                                    |
|:-------------------------------|:------------------------------------------------------|:---------------------------------------------------------------|
| **Windows x64/x86**            | `OpenCvSharp4.runtime.win`                            | Full native bindings for Windows (recommended)                 |
| **Windows x64/x86 (CUDA)**     | `OpenCvSharp4.runtime.win.cuda`                       | CUDA version of full native bindings for Windows (recommended) |
| **Windows x64/x86 (Slim)**     | `OpenCvSharp4.runtime.win.slim`                       | Slim bindings with core modules only                           |
| **UWP (x64/x86/ARM)**          | `OpenCvSharp4.runtime.uwp`                            | Native bindings for Universal Windows Platform                 |
| **macOS x64 (10.15+)**         | `OpenCvSharp4.runtime.osx.10.15-x64`                  | Native bindings for macOS x64 (10.15 Catalina and later)       |
| **macOS ARM64**                | `OpenCvSharp4.runtime.osx_arm64`                      | Native bindings for macOS ARM64 (Apple Silicon)                |
| **Linux x64 (Portable)**       | `OpenCvSharp4.official.runtime.linux-x64`             | Portable Linux x64 bindings (recommended)                      |
| **Linux x64 (Portable, Slim)** | `OpenCvSharp4.official.runtime.linux-x64.slim`        | Slim bindings for Linux x64 with core modules                  |
| **Ubuntu 22.04 x64**           | `OpenCvSharp4.official.runtime.ubuntu.22.04-x64`      | Native bindings for Ubuntu 22.04 x64                           |
| **Ubuntu 22.04 x64 (Slim)**    | `OpenCvSharp4.official.runtime.ubuntu.22.04-x64.slim` | Slim bindings for Ubuntu 22.04 x64                             |
| **Ubuntu 24.04 x64**           | `OpenCvSharp4.official.runtime.ubuntu.24.04-x64`      | Native bindings for Ubuntu 24.04 x64                           |
| **Ubuntu 24.04 x64 (Slim)**    | `OpenCvSharp4.official.runtime.ubuntu.24.04-x64.slim` | Slim bindings for Ubuntu 24.04 x64                             |
| **Linux ARM**                  | `OpenCvSharp4.runtime.linux-arm`                      | Native bindings for Linux ARM                                  |
| **WebAssembly**                | `OpenCvSharp4.runtime.wasm`                           | Native bindings for WebAssembly                                |

**Project Configuration Example:**

```xml
<ItemGroup Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('Windows'))) Or $(RuntimeIdentifier.StartsWith('win')) ">
    <PackageReference Include="OpenCvSharp4.runtime.win" Version="4.13.0.20260228"/>
    <!--  or OpenCvSharp4.runtime.win if you use CUDA  -->
</ItemGroup>

<ItemGroup
Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('OSX')) And '$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)' == 'Arm64') Or '$(RuntimeIdentifier)' == 'osx-arm64' ">
<PackageReference Include="OpenCvSharp4.runtime.osx_arm64" Version="4.13.0.20260228"/>
</ItemGroup>

<ItemGroup
Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('OSX')) And '$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)' != 'Arm64') Or '$(RuntimeIdentifier)' == 'osx-x64' Or '$(RuntimeIdentifier)' == 'osx.10.15-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.osx.10.15-x64" Version="4.13.0.20260228"/>
</ItemGroup>

<ItemGroup Condition=" '$(RuntimeIdentifier)' == 'linux-musl-x64' Or '$(RuntimeIdentifier)' == 'alpine-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.alpine-x64" Version="4.13.0.20260228"/>
</ItemGroup>

<ItemGroup Condition=" '$(RuntimeIdentifier)' == 'ubuntu.24.04-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.ubuntu.24.04-x64" Version="4.13.0.20260228"/>
</ItemGroup>
<ItemGroup Condition=" '$(RuntimeIdentifier)' == 'ubuntu.22.04-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.ubuntu.22.04-x64" Version="4.13.0.20260228"/>
</ItemGroup>
<ItemGroup Condition=" '$(RuntimeIdentifier)' == 'ubuntu.20.04-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.ubuntu.20.04-x64" Version="4.13.0.20260228"/>
</ItemGroup>
<ItemGroup Condition=" '$(RuntimeIdentifier)' == 'debian.12-x64' ">
<PackageReference Include="OpenCvSharp4.runtime.debian.12-x64" Version="4.13.0.20260228"/>
</ItemGroup>

<ItemGroup
Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('Linux')) And '$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)' == 'X64') Or ('$(RuntimeIdentifier)' == 'linux-x64') ">
<PackageReference Include="OpenCvSharp4.runtime.ubuntu.22.04-x64" Version="4.13.0.20260228"/>
</ItemGroup>

<ItemGroup
Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('Linux')) And '$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)' == 'Arm64') Or '$(RuntimeIdentifier)' == 'linux-arm64' ">
<PackageReference Include="OpenCvSharp4.runtime.linux_arm64" Version="4.13.0.20260228"/>
</ItemGroup>
<ItemGroup
Condition=" ('$(RuntimeIdentifier)' == '' And $([MSBuild]::IsOSPlatform('Linux')) And '$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)' == 'Arm') Or '$(RuntimeIdentifier)' == 'linux-arm' ">
<PackageReference Include="OpenCvSharp4.runtime.linux-arm" Version="4.13.0.20260228"/>
</ItemGroup>
```

## Usage

### ☁️ Cloud Module

Resolve shareable links to direct raw image streams.

```csharp
using SlideGenerator.Framework.Cloud;

var directUrl = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/...");
```

### 📊 Sheet Module

Read data from spreadsheets.

```csharp
using SlideGenerator.Framework.Sheet.Models;

using var workbook = new Workbook("data.xlsx");
var sheet = workbook.Worksheets["Sheet1"];
var rowData = sheet.GetRow(1); // Returns Dictionary<string, object>
```

### 🖼️ Slide Module

The core generation logic.

```csharp
using SlideGenerator.Framework.Slide.Models;
using SlideGenerator.Framework.Slide;

// 1. Load Template & Create Output
using var template = new TemplatePresentation("template.pptx");
using var working = template.SaveAs("output.pptx");

// 2. Clone a slide from template
var slidePart = working.CopySlide(template.MainSlideRelationshipId);

// 3. Replace Text
var replacements = new Dictionary<string, string>
{
    ["Name"] = "Alice",
    ["Title"] = "Engineer"
};
await TextReplacer.ReplaceAsync(slidePart, replacements);

// 4. Replace Image (by Shape ID)
var shapeId = 4U; // Discovered via template.GetAllPreviewImageShapes()
var shape = Presentation.GetShapeById(slidePart, shapeId);
using var imgStream = File.OpenRead("photo.png");

ImageReplacer.ReplaceImage(slidePart, shape!, imgStream);

// 5. Save
working.Save();
```

### 🧠 Image Module

**Face Detection with Provider Pattern**

```csharp
using OpenCvSharp;
using SlideGenerator.Framework.Features.Image.Contracts;
using SlideGenerator.Framework.Features.Image.Services;
using SlideGenerator.Framework.Features.Image.Entities.FaceDetection;
using ImageMagick;

// Setup with Dependency Injection (recommended)
services.AddSingleton<IFaceDetectorModelFactory, FaceDetectorModelFactory>();
services.AddSingleton<FaceDetectorModelManager>();
services.AddSingleton<IFaceDetectorModelProvider>(sp => 
    sp.GetRequiredService<FaceDetectorModelManager>());

// Use in your service
public class MyService(IFaceDetectorModelProvider faceDetectorProvider)
{
    public async Task<List<FaceInfo>> DetectFaces(string imagePath)
    {
        // Load image using Framework's ConvertingService
        using var magickImage = new MagickImage(imagePath);
        var mat = ConvertingService.ConvertImageToMat(magickImage);
        if (mat == null || mat.Empty())
            return new List<FaceInfo>();
        
        // Get current model (lazy initialization)
        var model = await faceDetectorProvider.GetCurrentModelAsync();
        
        // Detect faces - framework handles preprocessing automatically
        // Input: any size image
        // Internal: resizes to 320×320 with padding, detects, unmaps coordinates back
        var faces = await model.DetectAsync(mat);
        
        // Filter by confidence score
        return faces.Where(f => f.Confidence >= 0.7f).ToList();
    }
}
```

**ROI (Region of Interest) Calculation**

```csharp
using SlideGenerator.Framework.Features.Image.Entities.Roi;
using SlideGenerator.Framework.Features.Image.Models.Roi;
using SlideGenerator.Framework.Features.Image.Services;
using ImageMagick;

public class ImageProcessor(IFaceDetectorModelProvider faceDetectorProvider)
{
    public async Task<byte[]> CropAndResizeImage(string imagePath, Size targetSize)
    {
        // Load image using Framework
        using var magickImage = new MagickImage(imagePath);
        var mat = ConvertingService.ConvertImageToMat(magickImage);
        if (mat == null || mat.Empty())
            return Array.Empty<byte>();
        
        try
        {
            // Get ROI calculator with face detection support
            var calculator = await RoiType.RuleOfThirds.GetCalculator(faceDetectorProvider);
            
            // Calculate crop rectangle
            var cropRect = await calculator.CalculateRoiAsync(mat, targetSize);
            
            // Apply crop using Framework
            ManipulatingService.Crop(ref mat, cropRect);
            
            // Resize using Framework
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

**Image Manipulation**

```csharp
using SlideGenerator.Framework.Features.Image.Services;
using OpenCvSharp;
using ImageMagick;

public class ImageManipulation
{
    public void ManipulateImage(string imagePath)
    {
        // Load using Framework
        using var magickImage = new MagickImage(imagePath);
        var mat = ConvertingService.ConvertImageToMat(magickImage);
        if (mat == null) return;
        
        try
        {
            // All operations use OpenCvSharp.Size (not System.Drawing.Size)
            var targetSize = new Size(1920, 1080);
            
            // Resize with aspect ratio preservation (in-place)
            ManipulatingService.Resize(ref mat, targetSize, InterpolationFlags.Linear);
            
            // Crop to rectangle (in-place)
            var cropRect = new Rectangle(100, 100, 800, 600);
            ManipulatingService.Crop(ref mat, cropRect);
            
            // Clamp rectangle to border
            var border = new Rectangle(0, 0, mat.Width, mat.Height);
            var clampedRect = ManipulatingService.ClampToBorder(cropRect, border);
            
            // Get max aspect size
            var maxSize = ManipulatingService.GetMaxAspectSize(
                new Size(1920, 1080), 
                new Size(800, 800));
            
            // Convert back using Framework
            var resultBytes = ConvertingService.ConvertMatToImage(mat);
        }
        finally
        {
            mat.Dispose();
        }
    }
}

**Key Features**

- **YuNet Face Detection**: Fast ONNX model with 5 facial landmarks (eyes, nose, mouth)
- **Automatic Preprocessing**: Input normalization with resize + padding to 320×320
- **Coordinate Unmapping**: Detection results mapped back to original image coordinates
- **Provider Pattern**: Dependency injection support with `IFaceDetectorModelProvider`
- **ROI Algorithms**: Center, Saliency-based, and Face-aware (Rule of Thirds)
- **Singleton ROI**: `CenterRoi` and `ProminentRoi` are stateless singletons
- **All operations use OpenCvSharp types**: Consistent API across the framework

## Star History

<a href="https://www.star-history.com/#thnhmai06/SlideGenerator.Framework&type=timeline&legend=top-left">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=thnhmai06/SlideGenerator.Framework&type=timeline&theme=dark&legend=top-left" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=thnhmai06/SlideGenerator.Framework&type=timeline&legend=top-left" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=thnhmai06/SlideGenerator.Framework&type=timeline&legend=top-left" />
 </picture>
</a>

## Contributors

| [<img src="https://github.com/thnhmai06.png" width="100"><br><sub>**thnhmai06**</sub>](https://github.com/thnhmai06) | [<img src="https://github.com/Hair-Nguyeenx.png" width="100"><br><sub>**Hair-Nguyeenx**</sub>](https://github.com/Hair-Nguyeenx) |
|:--------------------------------------------------------------------------------------------------------------------:|:--------------------------------------------------------------------------------------------------------------------------------:|
|                             <span title="Project Manager">👑 <span title="Developer">💻                              |                                                    <span title="Developer">💻                                                    |
