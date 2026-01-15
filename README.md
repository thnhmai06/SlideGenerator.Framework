# SlideGenerator.Framework

A .NET framework for generating PowerPoint files from Excel data.

Docs:

- [English](docs/en)
- [Vietnamese](docs/vi)

Quick links:

- [Overview](docs/en/overview.md)
- [Usage](docs/en/usage.md)

## Modules

- Cloud: resolve supported cloud URLs to direct download links.
- Sheet: workbook and worksheet access for Excel/CSV data.
- Slide: template loading, slide cloning, text replacement, image replacement.
- Image: ROI detection, cropping, and resizing helpers.

## Prerequisites

### EmguCV Runtime

This framework relies on **EmguCV** for advanced image processing (ROI detection, face detection). You **must** ensure that the appropriate native runtime package for your target architecture is installed in your final application project:

- **Windows (x64):** `Emgu.CV.runtime.windows`
- **Linux (x64):** `Emgu.CV.runtime.ubuntu-x64` (or other corresponding Linux runtimes)

> For more details, see [Emgu.CV Installation](https://www.emgu.com/wiki/index.php/Download_And_Installation).

Ensure the runtime version matches the `Emgu.CV` version used in the framework (currently `4.12.0.5764`). Without the proper runtime, image processing features will fail with `DllNotFoundException` or `TypeInitializationException`.

You can configure your `.csproj` to automatically include the correct runtime based on the target `RuntimeIdentifier`:

```xml
<ItemGroup Condition="'$(RuntimeIdentifier)'=='win-x64'">
	<PackageReference Include="Emgu.CV.runtime.windows" Version="4.12.0.5764" />
</ItemGroup>
<ItemGroup Condition="'$(RuntimeIdentifier)'=='linux-x64'">
	<PackageReference Include="Emgu.CV.runtime.ubuntu-x64" Version="4.12.0.5764" />
</ItemGroup>
<ItemGroup Condition="'$(RuntimeIdentifier)'=='linux-arm'">
	<PackageReference Include="Emgu.CV.runtime.debian-arm" Version="4.12.0.5764" />
</ItemGroup>
<ItemGroup Condition="'$(RuntimeIdentifier)'=='linux-arm64'">
	<PackageReference Include="Emgu.CV.runtime.debian-arm64" Version="4.12.0.5764" />
</ItemGroup>
```

**Note:** macOS is **not supported** in these script because the Emgu.CV runtime does not support it directly out of the box in this context. For more details, see [Emgu.CV MacOS installation](https://www.emgu.com/wiki/index.php/Download_And_Installation#Mac_OS).

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

- Template presentations must contain exactly one slide; the template index is fixed at 1.
- When the template has more than one slide, `NotOnlyOneSlidePresentation` is thrown.
- Use `GetAllPreviewImageShapes()` to discover image shape ids and previews.
- Use `ImageReplacer.ReplaceImage(...)` for image placeholders.
- Call `CopySlide(...)` for each row, then `Save()` the working presentation.

## Image

Key types:

- `SlideGenerator.Framework.Image.Models.Image`
- `SlideGenerator.Framework.Image.Modules.FaceDetection.Models.FaceDetectorModel`
- `SlideGenerator.Framework.Image.Modules.FaceDetection.Models.YuNetModel`
- `SlideGenerator.Framework.Image.Modules.Roi.RoiModule`
- `SlideGenerator.Framework.Image.Modules.Roi.Configs.RoiOptions`
- `SlideGenerator.Framework.Image.Modules.Roi.Enums.RoiType`
- `SlideGenerator.Framework.Image.Modules.Roi.Enums.CropType`

Usage:

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

Notes:

- Face model init is async and serialized inside `YuNetModel`.
- Exceptions are thrown for unsupported or invalid inputs; callers should catch and handle.

## Contributors:

- **Leader: [@thnhmai06](https://github.com/thnhmai06)**
- [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)
