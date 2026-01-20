# Framework Usage Guide

[🇻🇳 Vietnamese Version](../vi/usage.md)

This guide provides detailed examples for using the individual modules of the `SlideGenerator.Framework`.

## ☁️ Cloud Module

Resolve direct download links from supported cloud storage providers.

**Supported Services:**
- Google Drive
- OneDrive
- Google Photos

```csharp
using SlideGenerator.Framework.Cloud;

// Resolves a shareable link to a direct URI
var directUri = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/123xyz/view");

// You can now download the stream
using var httpClient = new HttpClient();
using var stream = await httpClient.GetStreamAsync(directUri);
```

## 📊 Sheet Module

Read data from Excel files efficiently.

```csharp
using SlideGenerator.Framework.Sheet.Models;

// Open the workbook (disposes file stream when done)
using var workbook = new Workbook("C:\\data\\source.xlsx");

// Get a summary of all sheets
var sheetInfos = workbook.GetWorksheetsInfo(); // Returns List<WorksheetInfo>

// Access a specific sheet
var sheet = workbook.Worksheets["Sheet1"];

// Read a row (1-based index)
// Returns Dictionary<string, object> where keys are column headers
var rowData = sheet.GetRow(1); 

if (rowData.ContainsKey("Name"))
{
    Console.WriteLine($"Name: {rowData["Name"]}");
}
```

## 🖼️ Slide Module

The core presentation manipulation logic.

### 1. Setup

```csharp
using SlideGenerator.Framework.Slide.Models;

// Load the template (must act as the source)
using var template = new TemplatePresentation("template.pptx");

// Create a working copy for output
using var working = template.SaveAs("output.pptx");
```

> **Constraint:** The Template PPTX must contain exactly **one** slide.

### 2. Slide Cloning & Management

```csharp
// Inspect the template to find image placeholders
// Returns a dictionary of ShapeID -> Image Bytes (preview)
var previews = template.GetAllPreviewImageShapes();
var targetShapeId = previews.Keys.First(); 

// Clone the template's slide into the working presentation
// This creates a new slide at position 2 (after the title slide if any, or at end)
var slidePart = working.CopySlide(template.MainSlideRelationshipId, position: 2);
```

### 3. Text Replacement

Replaces `{{Key}}` patterns with values. Keys in the dictionary should **not** contain braces.

```csharp
using SlideGenerator.Framework.Slide;

var replacements = new Dictionary<string, string>
{
    ["FullName"] = "Alice Smith",
    ["Role"] = "Software Engineer"
};

var (count, details) = await TextReplacer.ReplaceAsync(slidePart, replacements);
Console.WriteLine($"Replaced {count} text instances.");
```

### 4. Image Replacement

Replaces an image shape with new content while preserving layout.

```csharp
using SlideGenerator.Framework.Slide;

// Find the specific shape on the cloned slide
var shape = Presentation.GetShapeById(slidePart, targetShapeId);

if (shape != null)
{
    using var newImageStream = File.OpenRead("profile.jpg");
    ImageReplacer.ReplaceImage(slidePart, shape, newImageStream);
}
```

### 5. Finalize

```csharp
// Optional: Remove the original template slide if it was copied to the beginning
// working.RemoveSlide(1);

// Save changes to disk
working.Save();
```

## 🧠 Image Module

Advanced image processing using EmguCV.

### Face Detection & Smart Cropping

```csharp
using SlideGenerator.Framework.Image.Models;
using SlideGenerator.Framework.Image.Modules.FaceDetection.Models;
using SlideGenerator.Framework.Image.Modules.Roi;
using SlideGenerator.Framework.Image.Modules.Roi.Configs;
using SlideGenerator.Framework.Image.Modules.Roi.Enums;

// 1. Initialize the Face Detector (YuNet)
using var faceModel = new YuNetModel();
// Note: Model loading is async internal logic, usually happens on first use

// 2. Configure ROI (Region of Interest) Module
var roiOptions = new RoiOptions { SaliencyPadding = 0.1f };
var roiModule = new RoiModule(roiOptions)
{
    FaceDetectionModel = faceModel
};

// 3. Load Image
using var image = new Image("input.jpg");

// 4. Select ROI Strategy (e.g., Focus on Face, or Rule of Thirds)
var selector = roiModule.GetRoiSelector(RoiType.Face);

// 5. Crop
// Crops the image to 500x500, focusing on the detected face
await RoiModule.CropToRoiAsync(image, new Size(500, 500), selector, CropType.Fill);

// 6. Save or use the modified image
image.Save("cropped.jpg");
```

**Note:** Ensure the correct EmguCV runtime is installed for your OS.
