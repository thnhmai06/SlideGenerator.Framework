# SlideGenerator.Framework
Framework .NET hỗ trợ tạo generate file PowerPoint từ dữ liệu file Excel.

Vietnamese | [English](/README.md)

## Tính năng
- [Thay thế Văn bản, Hình ảnh](API/slides.md)
- [Cắt ảnh theo ROI](API/images.md) - bao gồm nhận diện khuôn mặt và cắt theo độ nổi bật
- [Đọc dữ liệu từ file Excel](API/sheets.md)
- [Resolve link ảnh từ file Cloud](API/cloud.md)
  
## Yêu cầu
- .NET 10
- Runtime tương ứng với [EmguCV](https://www.emgu.com/wiki/index.php/Download_And_Installation) (Bản `mini` là đủ)

## Cài đặt
- NuGet: `dotnet add package SlideGenerator.Framework`
- Từ Source: Thêm tham chiếu dự án tới `SlideGenerator.Framework.csproj`.

## Bắt đầu nhanh

### Khái niệm
- **Placeholder**: token Mustache `{{ten}}`; dùng `TextReplacer.ScanPlaceholders` để dò trước khi thay.
- **Crop ảnh**: chọn từ ba chế độ ROI:
  - `RoiType.Prominent` - cắt theo vùng nổi bật nhất bằng phân tích spectral residual
  - `RoiType.Center` - cắt đơn giản ở giữa
  - `RoiType.Attention` - cắt thông minh kết hợp nhận diện khuôn mặt với độ nổi bật để tối ưu cho ảnh người
- **Slide**: `TemplatePresentation` yêu cầu chứa đúng 1 slide; `DerivedPresentation` sao chép slide, thay văn bản/hình và lưu file.
- **Sheet**: `Workbook` dùng ClosedXML; mỗi dòng là dictionary theo header; nếu thiếu sheet sẽ ném `WorksheetNotFoundException`.

### Ví dụ
1) Thay văn bản và hình trên slide mẫu

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

TextReplacer.Replace(slidePart, new() { ["title"] = "Báo cáo quý", ["owner"] = "Nhóm Dữ liệu" });

// Tạo ImageProcessor với RoiOptions tùy chỉnh cho nhận diện khuôn mặt
// ExpandRatio hỗ trợ padding riêng theo 4 hướng: trên, dưới, trái, phải
var roiOptions = new RoiOptions 
{ 
    FaceConfidence = 0.7f,
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // Nhiều khoảng trống cho tóc
        bottom: 0.15f,  // Ít hơn phía dưới
        left: 0.20f,
        right: 0.20f
    ),
    FacesUnionAll = true,
    SaliencyPaddingRatio = new ExpandRatio(0.0f)  // Padding đồng nhất
};
using var processor = new ImageProcessor(roiOptions);
await processor.InitFaceModelAsync(); // Khởi tạo mô hình nhận diện khuôn mặt

var picture = Presentation.GetPictures(slidePart).First();
var targetSize = ImageReplacer.GetPictureSize(picture);
using var img = new ImageData("photo.jpg");

// Dùng chế độ Attention để cắt thông minh kết hợp khuôn mặt + độ nổi bật
var roi = processor.GetRoi(img, targetSize, RoiType.Attention);
ImageProcessor.Crop(img, roi);

using var stream = new MemoryStream(img.ToByteArray());
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
using SlideGenerator.Framework.Cloud;

using var http = new HttpClient();
var directUrl = await CloudUrlResolver.ResolveAsync("https://drive.google.com/file/d/abc/view", http);
```

### Ghi chú
- Bản FreeSpire giới hạn 10 slide (phù hợp với library này).
- Luôn gọi `Dispose` cho các đối tượng để giải phóng native resources.
- Nhận diện khuôn mặt (cho `RoiType.Attention`) yêu cầu gọi `InitFaceModelAsync()` trước khi sử dụng.
- `ExpandRatio` hỗ trợ nhiều cách cấu hình padding:
  - `new ExpandRatio(allSides)` - padding đồng nhất
  - `new ExpandRatio(vertical, horizontal)` - padding dọc/ngang
  - `new ExpandRatio(top, bottom, left, right)` - padding riêng 4 hướng

## Người đóng góp

- Phát triển Chính: [@thnhmai06](https://github.com/thnhmai06)
- Logic cho Cloud: [@Hair-Nguyeenx](https://github.com/Hair-Nguyeenx)