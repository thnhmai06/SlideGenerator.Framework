# Images API

## Loading and saving
- `new ImageData(path)` or `new ImageData(byte[])` loads into an OpenCV `Mat`.
- `ImageData.Save(path?)` writes PNG; `ToByteArray()` returns PNG bytes.

## Cropping and ROI
- `ImageProcessor.GetRoi(image, RoiType, Size)` returns a rectangle for the target size.
- `ImageProcessor.Crop(image, roi)` crops in place; `CropToRoiCopy(...)` returns a new image.
- ROI modes: `RoiType.Prominent` (spectral residual saliency) or `RoiType.Center`.

## Using in slides
- Get target size: `ImageReplacer.GetPictureSize(...)` or `GetShapeSize(...)` (see [Slides](slides.md)).
- Replace image data: `ImageReplacer.ReplaceImage(SlidePart, Picture|Shape, Stream)` with a PNG stream.

## Dispose
- Dispose `ImageData` to free native buffers.
