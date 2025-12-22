# Hướng dẫn sử dụng

## Mục lục

1. [Cloud](#cloud)
2. [Sheet](#sheet)
3. [Slide](#slide)
4. [Image](#image)

## Cloud

```csharp
var uri = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/.../view");
```

Hỗ trợ: Google Drive, OneDrive, Google Photos.

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

Ghi chú:

- Template phải chỉ có đúng 1 slide; index cố định là 1.
- Nếu template có nhiều slide sẽ ném `NotOnlyOneSlidePresentation`.

## Image

```csharp
var processor = new ImageProcessor(new RoiOptions());
var selector = processor.GetRoiSelector(RoiType.Center);
await ImageProcessor.CropToRoiAsync(imageData, targetSize, selector, CropType.Crop);
```
