# SlideGenerator.Framework

A .NET framework for generating PowerPoint files from Excel data.

Docs:

- English: [docs/en](docs/en)
- Vietnamese: [docs/vi](docs/vi)

Quick links:

- Overview: [docs/en/overview.md](docs/en/overview.md)
- Usage: [docs/en/usage.md](docs/en/usage.md)

## Modules

- Cloud: resolve supported cloud URLs to direct download links.
- Sheet: workbook and worksheet access for Excel/CSV data.
- Slide: template loading, slide cloning, text replacement, image replacement.
- Image: ROI detection, cropping, and resizing helpers.

## Cloud

Key type:

- `SlideGenerator.Framework.Cloud.CloudUrlResolver`

Usage:

```csharp
var uri = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/.../view");
```

Supported services:

- Google Drive
- OneDrive
- Google Photos

## Sheet

Key types:

- `SlideGenerator.Framework.Sheet.Models.Workbook`
- `SlideGenerator.Framework.Sheet.Contracts.IWorksheet`

Usage:

```csharp
using var workbook = new Workbook("data.xlsx");
var sheets = workbook.GetWorksheetsInfo();
var firstSheet = workbook.Worksheets["Sheet1"];
var row = firstSheet.GetRow(1);
```

Notes:

- `Workbook` is `IDisposable`. Dispose it when done.

## Slide

Key types:

- `SlideGenerator.Framework.Slide.Models.TemplatePresentation`
- `SlideGenerator.Framework.Slide.Models.WorkingPresentation`
- `SlideGenerator.Framework.Slide.TextReplacer`
- `SlideGenerator.Framework.Slide.ImageReplacer`

Usage:

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

- Template presentations must contain exactly one slide; the template index is fixed at 1.
- When the template has more than one slide, `NotOnlyOneSlidePresentation` is thrown.
- Use `GetAllPreviewImageShapes()` to discover image shape ids and previews.
- Use `ImageReplacer.ReplaceImage(...)` for image placeholders.

## Image

Key types:

- `SlideGenerator.Framework.Image.ImageProcessor`
- `SlideGenerator.Framework.Image.Configs.RoiOptions`
- `SlideGenerator.Framework.Image.Enums.RoiType`
- `SlideGenerator.Framework.Image.Enums.CropType`

Usage:

```csharp
var processor = new ImageProcessor(new RoiOptions());
var selector = processor.GetRoiSelector(RoiType.Center);
await ImageProcessor.CropToRoiAsync(imageData, targetSize, selector, CropType.Crop);
```

Notes:

- Face model init is async and serialized inside `ImageProcessor`.
- Exceptions are thrown for unsupported or invalid inputs; callers should catch and handle.

## Constraints

- Do not reimplement framework functionality in the backend.
- Do not add third-party libraries to replace framework behavior.
- Treat the framework as the source of truth for workbook, slide, image, and cloud operations.

## Contributors:

- **Main Developer: [@thnhmai06](https://github.com/thnhmai06)**
- Cloud Logic: [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)
