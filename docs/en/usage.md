# Usage

Vietnamese version: [Vietnamese](../vi/usage.md)

## Table of contents

- [Usage](#usage)
  - [Table of contents](#table-of-contents)
  - [Cloud](#cloud)
  - [Sheet](#sheet)
  - [Slide](#slide)
  - [Image](#image)

## Cloud

```csharp
var uri = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/.../view");
```

Supported services: Google Drive, OneDrive, Google Photos.

## Sheet

```csharp
using var workbook = new Workbook("data.xlsx");
var sheets = workbook.GetWorksheetsInfo();
var firstSheet = workbook.Worksheets["Sheet1"];
var row = firstSheet.GetRow(1);
```

## Slide

```csharp
using var template = new TemplatePresentation("template.pptx");
using var working = template.SaveAs("output.pptx");

// Discover image placeholders from the template.
var previews = template.GetAllPreviewImageShapes();
var shapeId = previews.Keys.First(); // example

// Clone the template slide for each output slide (per data row).
// position: 1-based. If omitted, it appends to the end.
var slidePart = working.CopySlide(template.MainSlideRelationshipId, position: 2);

// Replace text placeholders (use keys without {{ }}).
await TextReplacer.ReplaceAsync(slidePart, new Dictionary<string, string>
{
    ["Name"] = "Alice",
    ["Title"] = "Engineer"
});

// Replace image by shape id.
var shape = Presentation.GetShapeById(slidePart, shapeId);
using var png = File.OpenRead("photo.png");
ImageReplacer.ReplaceImage(slidePart, shape!, png);

// Remove the original template slide (now duplicated at the beginning).
working.RemoveSlide(1);
working.Save();
```

Notes:

- Template must contain exactly one slide; index is fixed at 1.
- `NotOnlyOneSlidePresentation` is thrown when the template has multiple slides.
- Use `GetAllPreviewImageShapes()` to discover image placeholders.
- Call `CopySlide(...)` for each data row, then `Save()` the working presentation.

## Image

```csharp
using var image = new Image("photo.png");
using var faceDetection = new YuNetModel();
var roi = new RoiModule(new RoiOptions())
{
    FaceDetectionModel = faceDetection
};

var selector = roi.GetRoiSelector(RoiType.Center);
await RoiModule.CropToRoiAsync(image, targetSize, selector, CropType.Crop);
```
