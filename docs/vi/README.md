# SlideGenerator.Framework
Framework .NET hỗ trợ tạo generate file PowerPoint từ dữ liệu file Excel.

Vietnamese | [English](/README.md)

## Tính năng
- [Thay thế Văn bản, Hình ảnh](API/slides.md)
- [Cắt ảnh theo ROI](API/images.md)
- [Đọc dữ liệu từ file Excel](API/sheets.md)
- [Resolve link ảnh từ file Cloud](API/cloud.md)
  
## Yêu cầu
- .NET 10
- Runtime tương ứng với [EmguCV](https://www.emgu.com/wiki/index.php/Download_And_Installation)

## Cài đặt
- NuGet: `dotnet add package SlideGenerator.Framework`
- Từ Source: Thêm tham chiếu dự án tới `SlideGenerator.Framework.csproj`.

## Bắt đầu nhanh

### Khái niệm
- Placeholder: token Mustache `{{ten}}`; dùng `TextReplacer.ScanPlaceholders` để dò trước khi thay.
- Crop ảnh: chọn `RoiType.Prominent` (nổi bật nhất) hoặc `RoiType.Center` (trung tâm), truyền kích thước đích theo pixel.
- Slide: `TemplatePresentation` yêu cầu chứa đúng 1 slide; `DerivedPresentation` sao chép slide, thay văn bản/hình và lưu file.
- Sheet: `Workbook` dùng ClosedXML; mỗi dòng là dictionary theo header; nếu thiếu sheet sẽ ném `WorksheetNotFoundException`.

### Ví dụ
1) Thay văn bản và hình trên slide mẫu
```csharp
using SlideGenerator.Framework.Slide;
using SlideGenerator.Framework.Slide.Models;
using SlideGenerator.Framework.Image;
using SlideGenerator.Framework.Image.Enums;
using SlideGenerator.Framework.Image.Models;

using var template = new TemplatePresentation("template.pptx");
using var deck = new DerivedPresentation("output.pptx", template);
var slidePart = deck.GetSlidePart(template.MainSlideRelationshipId);

TextReplacer.Replace(slidePart, new() { ["title"] = "Báo cáo quý", ["owner"] = "Nhóm Dữ liệu" });

var picture = Presentation.GetPictures(slidePart).First();
var targetSize = ImageReplacer.GetPictureSize(picture);
using var img = new ImageData("photo.jpg");
using var cropped = ImageProcessor.CropToRoiCopy(img, RoiType.Prominent, targetSize);
using var stream = new MemoryStream(cropped.ToByteArray());
ImageReplacer.ReplaceImage(slidePart, picture, stream);

deck.Save();
```

2) Đọc dữ liệu Excel để map vào placeholder
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

3) Lấy link tải trực tiếp từ cloud
```csharp
using SlideGenerator.Framework.Http;

using var http = new HttpClient();
var directUrl = await CloudUrlResolver.ResolveAsync("https://drive.google.com/file/d/abc/view", http);
```

### Ghi chú
- Bản FreeSpire giới hạn 10 slide (phù hợp với library này).
- Gọi `Dispose` cho các đối tượng để giải phóng native resources.

## Người đóng góp

- Phát triển Chính: [@thnhmai06](https://github.com/thnhmai06)
- Logic cho Cloud: [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)