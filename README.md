# SlideGenerator.Framework

<p>
  <img src="https://img.shields.io/badge/.NET-10.0-512bd4?style=flat-square" alt=".NET 10" />
  <img src="https://img.shields.io/badge/EmguCV-4.9.0-orange?style=flat-square" alt="EmguCV" />
  <a href="https://www.codefactor.io/repository/github/thnhmai06/slidegenerator.framework"><img src="https://www.codefactor.io/repository/github/thnhmai06/slidegenerator.framework/badge" alt="CodeFactor" /></a>
  <a href="https://ghloc.vercel.app/thnhmai06/SlideGenerator.Framework">
            <img src="https://img.shields.io/endpoint?url=https://ghloc.vercel.app/api/thnhmai06/SlideGenerator.Framework/badge%3Ffilter=.ts$,.tsx$,.html$,.css$,.cs$%26format=human&style=flat-square&color=blue" alt="Lines of Code" />
        </a>
  <a href="https://github.com/thnhmai06/SlideGenerator.Framework/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/thnhmai06/SlideGenerator.Framework?style=flat-square" alt="License" />
  </a>
</p>

A powerful, standalone .NET library for orchestrating the generation of PowerPoint presentations from Excel data sources. It provides high-level abstractions for template manipulation, data extraction, and intelligent image processing.

**Documentation:** [English](docs/en) | [Tiếng Việt](docs/vi)

## Modules

The framework is composed of four core modules:

| Module       | Namespace                        | Description                                                                             |
| :----------- | :------------------------------- | :-------------------------------------------------------------------------------------- |
| **☁️ Cloud** | `SlideGenerator.Framework.Cloud` | Resolves direct download links from Google Drive, OneDrive, and Google Photos.          |
| **📊 Sheet** | `SlideGenerator.Framework.Sheet` | efficient reading of Excel (.xlsx) and CSV files.                                       |
| **🖼️ Slide** | `SlideGenerator.Framework.Slide` | PowerPoint manipulation: template loading, slide cloning, text/image replacement.       |
| **🧠 Image** | `SlideGenerator.Framework.Image` | Advanced image processing: Face detection, ROI (Region of Interest) cropping, resizing. |

## Prerequisites

### EmguCV Runtime

This framework relies on **EmguCV** for computer vision tasks. You **must** install the native runtime package matching your target OS in the consuming project.

| OS                | Package                        |
| :---------------- | :----------------------------- |
| **Windows (x64)** | `Emgu.CV.runtime.windows`      |
| **Linux (x64)**   | `Emgu.CV.runtime.ubuntu-x64`   |
| **Linux (ARM)**   | `Emgu.CV.runtime.debian-arm`   |
| **Linux (ARM64)** | `Emgu.CV.runtime.debian-arm64` |

> 🔗 [Official EmguCV Installation Guide](https://www.emgu.com/wiki/index.php/Download_And_Installation)

**Project Configuration Example:**

```xml
<ItemGroup Condition="'$(RuntimeIdentifier)'=='win-x64'">
    <PackageReference Include="Emgu.CV.runtime.windows" Version="4.12.0.5764" />
</ItemGroup>
<ItemGroup Condition="'$(RuntimeIdentifier)'=='linux-x64'">
    <PackageReference Include="Emgu.CV.runtime.ubuntu-x64" Version="4.12.0.5764" />
</ItemGroup>
```

> **Note:** macOS is currently **not supported** due to EmguCV runtime limitations in this context.

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

Intelligent cropping based on Region of Interest (ROI).

```csharp
using SlideGenerator.Framework.Image.Models;
using SlideGenerator.Framework.Image.Modules.Roi;

using var image = new Image("photo.png");
using var faceModel = new YuNetModel(); // Pre-trained face detector

var roiModule = new RoiModule(new RoiOptions { FaceDetectionModel = faceModel });
var selector = roiModule.GetRoiSelector(RoiType.Face);

// Crop image focusing on the face
await RoiModule.CropToRoiAsync(image, new Size(200, 200), selector, CropType.Fill);
```

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
| :------------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------: |
|                             <span title="Project Manager">👑 <span title="Developer">💻                              |                                                    <span title="Developer">💻                                                    |
