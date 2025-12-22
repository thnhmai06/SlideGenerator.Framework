# Usage

Vietnamese version: [Vietnamese](../vi/usage.md)

## Table of contents

1. [Cloud](#cloud)
2. [Sheet](#sheet)
3. [Slide](#slide)
4. [Image](#image)

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
var working = template.SaveAs("output.pptx");
var slidePart = template.GetSlidePart();

await TextReplacer.ReplaceAsync(slidePart, new Dictionary<string, string>
{
    ["{{Name}}"] = "Alice"
});
```

Notes:

- Template must contain exactly one slide; index is fixed at 1.
- `NotOnlyOneSlidePresentation` is thrown when the template has multiple slides.

## Image

```csharp
var processor = new ImageProcessor(new RoiOptions());
var selector = processor.GetRoiSelector(RoiType.Center);
await ImageProcessor.CropToRoiAsync(imageData, targetSize, selector, CropType.Crop);
```

