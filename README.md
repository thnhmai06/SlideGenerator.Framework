# SlideGenerator.Framework

A .NET framework for generating PowerPoint files from Excel data.

[Vietnamese](docs/vi/README.md) | English

## Features

- [Text and Image Replacement](docs/en/API/slides.md)
- [Image Cropping by ROI](docs/en/API/images.md) - including face detection and saliency-based cropping
- [Read Data from Excel Files](docs/en/API/sheets.md)
- [Resolve Image Links from Cloud Storage](docs/en/API/cloud.md)

## Requirements

- .NET 10
- The corresponding [EmguCV](https://www.emgu.com/wiki/index.php/Download_And_Installation) runtime (`mini` version is enough)

## Installation

- NuGet: `dotnet add package SlideGenerator.Framework`
- From source: add a project reference to `SlideGenerator.Framework.csproj`.

## Quick Start

### Concepts

- **Placeholder**: Mustache tokens like `{{name}}`; use `TextReplacer.ScanPlaceholders` to scan before replacing.
- **Image cropping**: choose from three ROI modes:
  - `RoiType.Prominent` - crops to the most visually salient area using spectral residual analysis
  - `RoiType.Center` - simple center crop
  - `RoiType.Attention` - intelligent cropping that combines face detection with saliency for optimal human-centric framing
- **Slide**: `TemplatePresentation` must contain exactly one slide; `DerivedPresentation` duplicates the slide, replaces text/images, and saves the output file.
- **Sheet**: `Workbook` is based on ClosedXML; each row is represented as a dictionary keyed by headers; if a worksheet is missing, a `WorksheetNotFoundException` is thrown.

### Examples

1. Replace text and images in a template slide

```csharp
using SlideGenerator.Framework.Slide;
using SlideGenerator.Framework.Slide.Models;
using SlideGenerator.Framework.Image;
using SlideGenerator.Framework.Image.Enums;
using SlideGenerator.Framework.Image.Models;
using SlideGenerator.Framework.Image.Configs;

using var template = new TemplatePresentation("template.pptx");
using var deck = new DerivedPresentation("output.pptx", template);
var slidePart = deck.GetSlidePart(template.MainSlideRelationshipId);

TextReplacer.Replace(slidePart, new() { ["title"] = "Quarterly Report", ["owner"] = "Data Team" });

// Create ImageProcessor with custom ROI options for face detection
// ExpandRatio supports 4-direction independent padding: top, bottom, left, right
var roiOptions = new RoiOptions 
{ 
    FaceConfidence = 0.7f,
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // More space for hair
        bottom: 0.15f,  // Less space below
        left: 0.20f,
        right: 0.20f
    ),
    FacesUnionAll = true,
    SaliencyPaddingRatio = new ExpandRatio(0.0f)  // Uniform padding
};
using var processor = new ImageProcessor(roiOptions);
await processor.InitFaceModelAsync(); // Initialize face detection model

var picture = Presentation.GetPictures(slidePart).First();
var targetSize = ImageReplacer.GetPictureSize(picture);
using var img = new ImageData("photo.jpg");

// Use Attention mode for intelligent face + saliency cropping
var getRoi = processor.GetRoiFunc(RoiType.Attention);
await ImageProcessor.CropToRoiAsync(img, targetSize, getRoi, CropType.Fit);

using var stream = new MemoryStream(img.ToByteArray());
ImageReplacer.ReplaceImage(slidePart, picture, stream);

deck.Save();
```

2. Read Excel data and map it to placeholders

```csharp
using SlideGenerator.Framework.Sheet.Models;

using var workbook = new Workbook("data.xlsx");
var sheet = workbook.GetWorksheet("Slides");
foreach (var row in sheet.GetAllRows())
{
    var placeholders = row; // header -> value
    // TextReplacer.Replace(...)
}
```

3. Resolve direct download links from cloud storage

```csharp
using SlideGenerator.Framework.Cloud;

using var http = new HttpClient();
var directUrl = await CloudUrlResolver.ResolveAsync("https://drive.google.com/file/d/abc/view", http);
```

### Notes

- The FreeSpire edition is limited to 10 slides (which fits this library's scope).
- Always call `Dispose` on objects to release native resources.
- Face detection (for `RoiType.Attention`) requires calling `InitFaceModelAsync()` before use.
- `ExpandRatio` supports flexible padding configurations:
  - `new ExpandRatio(allSides)` - uniform padding
  - `new ExpandRatio(vertical, horizontal)` - vertical/horizontal padding
  - `new ExpandRatio(top, bottom, left, right)` - independent 4-direction padding

## Contributors

- Main developer: [@thnhmai06](https://github.com/thnhmai06)
- Cloud logic:  [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)