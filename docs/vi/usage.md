# Hướng dẫn sử dụng

Phiên bản tiếng Anh: [English](../en/usage.md)

## Mục lục

- [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
  - [Mục lục](#mục-lục)
  - [Cloud](#cloud)
  - [Sheet](#sheet)
  - [Slide](#slide)
  - [Image](#image)

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
using var working = template.SaveAs("output.pptx");

// Lấy các placeholder ảnh từ template.
var previews = template.GetAllPreviewImageShapes();
var shapeId = previews.Keys.First(); // ví dụ

// Clone slide mẫu cho mỗi dòng dữ liệu.
// position: đánh số từ 1. Nếu bỏ trống thì sẽ thêm cuối.
var slidePart = working.CopySlide(template.MainSlideRelationshipId, position: 2);

// Thay thế text (dùng key không kèm {{ }}).
var (replacedCount, details) = await TextReplacer.ReplaceAsync(slidePart, new Dictionary<string, string>
{
    ["Name"] = "Alice",
    ["Title"] = "Engineer"
});

// Thay thế ảnh bằng shape id.
var shape = Presentation.GetShapeById(slidePart, shapeId);
using var png = File.OpenRead("photo.png");
ImageReplacer.ReplaceImage(slidePart, shape!, png);

// Xóa slide mẫu ở đầu, sau khi đã copy.
working.RemoveSlide(1);
working.Save();
```

Ghi chú:

- Template phải chỉ có đúng 1 slide; index cố định là 1.
- Nếu template có nhiều slide sẽ ném `NotOnlyOneSlidePresentation`.
- Dùng `GetAllPreviewImageShapes()` để lấy shape ảnh.
- Gọi `CopySlide(...)` cho mỗi dòng dữ liệu, sau đó `Save()` file.

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
