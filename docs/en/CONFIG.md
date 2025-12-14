# Image Processing Configuration Guide

## Overview
SlideGenerator Framework supports three ROI (Region of Interest) cropping modes:

1. **Center** - Simple center crop
2. **Prominent** - Finds and crops the most salient region using spectral residual analysis
3. **Attention** (New) - Combines face detection with saliency for optimal human-centric framing

## Attention Mode Configuration

Attention mode uses the following parameters in `backend.config.yaml`:

### Face Detection Settings (`image.face`)

#### `face.confidence` (default: 0.6)
- **Description**: Minimum confidence score (0-1) required to accept a detected face
- **Low values (0.3-0.5)**: Detect more faces, may include false positives
- **High values (0.7-0.9)**: Only accept high-confidence faces, fewer false positives
- **Recommendation**: 0.6 for most cases, 0.7-0.8 for higher precision

#### Face Padding (default per side: 0.15)
Padding around detected faces (0-1), relative to face size. Can be configured **independently for each direction**:

**`face.padding_top`**
- **Description**: Padding above the face
- **Example**: 0.15 = add 15% of face height above face
- **Recommendation**: 0.15-0.25 to include hair and forehead

**`face.padding_bottom`**
- **Description**: Padding below the face
- **Example**: 0.15 = add 15% of face height below face
- **Recommendation**: 0.15-0.20 to include neck and shoulders

**`face.padding_left`**
- **Description**: Padding to the left of the face
- **Example**: 0.15 = add 15% of face width to the left
- **Recommendation**: 0.15-0.20 for balanced framing

**`face.padding_right`**
- **Description**: Padding to the right of the face
- **Example**: 0.15 = add 15% of face width to the right
- **Recommendation**: 0.15-0.20 for balanced framing

**Note**: Padding values can differ between directions to create better composition (e.g., more top padding for portraits).

#### `face.union_all` (default: true)
- **Description**: How to handle multiple detected faces
- **true**: Union all faces into a single ROI region (suitable for group photos)
- **false**: Select only the best face (highest score, largest size) (suitable for individual portraits)
- **Recommendation**: true for group shots, false for individual portraits

### Saliency Settings (`image.saliency`)

#### Saliency Padding (default per side: 0.0)
Padding around saliency anchor (0-1), relative to crop window size. Can be configured **independently for each direction**:

**`saliency.padding_top`**, **`saliency.padding_bottom`**, **`saliency.padding_left`**, **`saliency.padding_right`**
- **Description**: Per-direction padding around salient region
- **0.0**: No padding, precise crop based on saliency + faces
- **> 0.0**: Expand ROI to include more context
- **Recommendation**: Keep at 0.0 in most cases, or slightly increase (0.05-0.1) for more context

## Configuration Examples

### Individual Portrait - High Precision
```yaml
image:
  face:
    confidence: 0.75
    padding_top: 0.25        # More padding for hair
    padding_bottom: 0.15     # Less below
    padding_left: 0.20
    padding_right: 0.20
    union_all: false
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

### Group Photo - Include All
```yaml
image:
  face:
    confidence: 0.55
    padding_top: 0.15
    padding_bottom: 0.15
    padding_left: 0.15
    padding_right: 0.15
    union_all: true
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

### Artistic Photo - Asymmetric Context
```yaml
image:
  face:
    confidence: 0.65
    padding_top: 0.30        # More space above
    padding_bottom: 0.20
    padding_left: 0.25       # More on left (rule of thirds)
    padding_right: 0.15      # Less on right
    union_all: true
  saliency:
    padding_top: 0.05
    padding_bottom: 0.05
    padding_left: 0.05
    padding_right: 0.05
```

### Landscape Photo - Optimized for Wide Format
```yaml
image:
  face:
    confidence: 0.60
    padding_top: 0.10        # Less vertical padding
    padding_bottom: 0.10
    padding_left: 0.25       # More horizontal padding for breathing room
    padding_right: 0.25
    union_all: true
  saliency:
    padding_top: 0.0
    padding_bottom: 0.0
    padding_left: 0.0
    padding_right: 0.0
```

## Notes

1. **Performance**: Attention mode requires additional processing time for face detection. The model is cached after first use.

2. **Fallback**: If no faces are detected, the system automatically falls back to Prominent mode (saliency only).

3. **Memory**: Face detection model requires ~10-20MB RAM when loaded.

4. **Thread-safe**: The model is initialized once and can be shared across multiple requests.

5. **Asymmetric padding**: Use different padding for different directions to create better composition following photography rules (rule of thirds, golden ratio, etc.)

## API Usage

### In Infrastructure (Automatic from config)
```csharp
// SlideGenerator and ImageService automatically read config
// No additional code needed
```

### In Framework (Manual customization)
```csharp
using SlideGenerator.Framework.Image;
using SlideGenerator.Framework.Image.Configs;
using SlideGenerator.Framework.Image.Models;

// Create RoiOptions with custom per-direction padding
var roiOptions = new RoiOptions 
{ 
    FaceConfidence = 0.7f,
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // 25% padding above
        bottom: 0.15f,  // 15% padding below
        left: 0.20f,    // 20% padding left
        right: 0.20f    // 20% padding right
    ),
    FacesUnionAll = true,
    SaliencyPaddingRatio = new ExpandRatio(0.0f)  // Uniform all sides
};

// Create ImageProcessor with options
using var processor = new ImageProcessor(roiOptions);

// Initialize model (asynchronous)
await processor.InitFaceModelAsync();

// Use it
using var image = new ImageData("photo.jpg");
var roi = processor.GetAttentionRoi(image, targetSize);
ImageProcessor.Crop(image, roi);
```

## Troubleshooting

### No faces detected?
- Lower `face.confidence` to 0.4-0.5
- Verify the image contains clear faces
- Try with higher resolution images

### ROI not as expected?
- Try adjusting padding per direction independently
- Increase `face.padding_top` if hair is cut off
- Increase `face.padding_bottom` if shoulders/neck are cut off
- Adjust `face.padding_left`/`face.padding_right` for better composition
- Toggle `face.union_all` between true/false
- Consider using `Prominent` mode if image has no people

### ROI shifted to one side?
- Use asymmetric padding to adjust
- Example: `face.padding_left: 0.30`, `face.padding_right: 0.10` to push subject right

### Performance slow?
- Attention mode is slower than Center and Prominent due to face detection
- Only use Attention for images with people
- Model is cached so first use will be slower than subsequent uses
