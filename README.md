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

## Prerequisites

### EmguCV Runtime

This framework relies on **EmguCV** for advanced image processing (ROI detection, face detection). You **must** ensure that the appropriate native runtime package for your target architecture is installed in your final application project:

- **Windows (x64):** `Emgu.CV.runtime.windows`
- **Linux (x64):** `Emgu.CV.runtime.ubuntu-x64` (or other corresponding Linux runtimes)

**Note:** macOS is currently **not supported** because the Emgu.CV runtime does not support it directly out of the box in this context. For more details on installing Emgu.CV runtimes, please visit: [Emgu.CV Download And Installation](https://www.emgu.com/wiki/index.php/Download_And_Installation).

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

## Contributors:

- **Main Developer: [@thnhmai06](https://github.com/thnhmai06)**
- Cloud Logic: [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)
