# Images API

## Loading and saving
- `new ImageData(path)` or `new ImageData(byte[])` loads into an OpenCV `Mat`.
- `ImageData.Save(path?)` writes PNG; `ToByteArray()` returns PNG bytes.

## Cropping and ROI
- `ImageProcessor.GetRoi(image, RoiType, Size)` returns a rectangle for the target size.
- `ImageProcessor.Crop(image, roi)` crops in place; `CropToRoiCopy(...)` returns a new image.
- ROI modes:
  - `RoiType.Prominent` - spectral residual saliency (most visually prominent region)
  - `RoiType.Center` - simple center crop
  - `RoiType.Attention` - combines face detection with saliency for human-centric cropping

## ROI Options
- `RoiOptions` configures face detection and padding for `RoiType.Attention`:
  - `FaceConfidence` (default: 0.6) - minimum confidence score (0-1) to accept face detections
  - `FacePaddingRatio` - padding around detected faces, can be configured independently for all 4 directions:
    - Constructor: `new ExpandRatio(top, bottom, left, right)` - independent per-direction padding
    - Constructor: `new ExpandRatio(vertical, horizontal)` - vertical/horizontal padding
    - Constructor: `new ExpandRatio(allSides)` - uniform padding
  - `FacesUnionAll` (default: true) - if true, union all faces; otherwise use best single face
  - `SaliencyPaddingRatio` - padding around saliency anchor, also supports 4-direction configuration
- Create `ImageProcessor` with custom options: `new ImageProcessor(roiOptions)`
- Face detection requires initialization: `await processor.InitFaceModelAsync()`

## Examples

### Uniform padding
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(0.15f)  // 15% all directions
};
```

### Vertical/horizontal padding
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(
        vertical: 0.20f,    // 20% top and bottom
        horizontal: 0.15f   // 15% left and right
    )
};
```

### Per-direction padding
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // 25% above
        bottom: 0.15f,  // 15% below
        left: 0.20f,    // 20% left
        right: 0.20f    // 20% right
    )
};
```

## Using in slides
- Get target size: `ImageReplacer.GetPictureSize(...)` or `GetShapeSize(...)` (see [Slides](slides.md)).
- Replace image data: `ImageReplacer.ReplaceImage(SlidePart, Picture|Shape, Stream)` with a PNG stream.

## Dispose
- Dispose `ImageData` to free native buffers.
- Dispose `ImageProcessor` to free face detection model resources.
