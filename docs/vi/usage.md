# Hướng dẫn sử dụng Framework

[🇺🇸 English Version](../en/usage.md)

Tài liệu này cung cấp các ví dụ chi tiết về cách sử dụng từng module của `SlideGenerator.Framework`.

## ☁️ Cloud Module

Phân giải đường dẫn chia sẻ từ các dịch vụ lưu trữ đám mây thành link tải trực tiếp.

**Dịch vụ hỗ trợ:**
- Google Drive
- OneDrive
- Google Photos

```csharp
using SlideGenerator.Framework.Cloud;

// Phân giải link chia sẻ thành URI trực tiếp
var directUri = await CloudUrlResolver.ResolveLinkAsync("https://drive.google.com/file/d/123xyz/view");

// Bây giờ bạn có thể tải luồng dữ liệu về
using var httpClient = new HttpClient();
using var stream = await httpClient.GetStreamAsync(directUri);
```

## 📊 Sheet Module

Đọc dữ liệu từ file Excel một cách hiệu quả.

```csharp
using SlideGenerator.Framework.Sheet.Models;

// Mở workbook (tự động dispose luồng file khi xong)
using var workbook = new Workbook("C:\\data\\source.xlsx");

// Lấy thông tin tóm tắt của tất cả các sheet
var sheetInfos = workbook.GetWorksheetsInfo(); // Trả về List<WorksheetInfo>

// Truy cập một sheet cụ thể
var sheet = workbook.Worksheets["Sheet1"];

// Đọc một dòng (chỉ số bắt đầu từ 1)
// Trả về Dictionary<string, object> với key là tiêu đề cột
var rowData = sheet.GetRow(1); 

if (rowData.ContainsKey("Name"))
{
    Console.WriteLine($"Name: {rowData["Name"]}");
}
```

## 🖼️ Slide Module

Logic cốt lõi để thao tác với bài thuyết trình.

### 1. Khởi tạo

```csharp
using SlideGenerator.Framework.Slide.Models;

// Tải template (đóng vai trò là nguồn)
using var template = new TemplatePresentation("template.pptx");

// Tạo bản sao làm việc (working copy) cho đầu ra
using var working = template.SaveAs("output.pptx");
```

> **Ràng buộc:** File PPTX template phải chứa chính xác **một** slide.

### 2. Clone Slide & Quản lý

```csharp
// Quét template để tìm các placeholder hình ảnh
// Trả về Dictionary của ShapeID -> Image Bytes (preview)
var previews = template.GetAllPreviewImageShapes();
var targetShapeId = previews.Keys.First(); 

// Clone slide từ template vào working presentation
// Lệnh này tạo slide mới ở vị trí 2 (sau slide tiêu đề nếu có, hoặc ở cuối)
var slidePart = working.CopySlide(template.MainSlideRelationshipId, position: 2);
```

### 3. Thay thế Văn bản

Thay thế các mẫu `{{Key}}` bằng giá trị thực. Key trong dictionary **không** được chứa dấu ngoặc nhọn.

```csharp
using SlideGenerator.Framework.Slide;

var replacements = new Dictionary<string, string>
{
    ["FullName"] = "Alice Smith",
    ["Role"] = "Software Engineer"
};

var (count, details) = await TextReplacer.ReplaceAsync(slidePart, replacements);
Console.WriteLine($"Đã thay thế {count} vị trí văn bản.");
```

### 4. Thay thế Hình ảnh

Thay thế một shape hình ảnh bằng nội dung mới trong khi vẫn giữ nguyên bố cục.

```csharp
using SlideGenerator.Framework.Slide;

// Tìm shape cụ thể trên slide đã clone
var shape = Presentation.GetShapeById(slidePart, targetShapeId);

if (shape != null)
{
    using var newImageStream = File.OpenRead("profile.jpg");
    ImageReplacer.ReplaceImage(slidePart, shape, newImageStream);
}
```

### 5. Hoàn tất

```csharp
// Tùy chọn: Xóa slide template gốc nếu nó đã bị copy lên đầu
// working.RemoveSlide(1);

// Lưu thay đổi xuống ổ đĩa
working.Save();
```

## 🧠 Image Module

Xử lý ảnh nâng cao sử dụng EmguCV.

### Nhận diện khuôn mặt & Cắt thông minh

```csharp
using Emgu.CV;
using SlideGenerator.Framework.Image.Entities.FaceDetection;

// 1. Tạo và khởi tạo model nhận diện khuôn mặt
using var faceModel = new YuNetModel();
if (!await faceModel.InitAsync())
    throw new InvalidOperationException("Khởi tạo model YuNet thất bại.");

// 2. Tải ảnh
using var mat = CvInvoke.Imread("input.jpg");

// 3. Nhận diện khuôn mặt
// DetectAsync sẽ throw InvalidOperationException nếu model chưa được khởi tạo.
// Framework trả về toàn bộ kết quả detect; lọc score do caller tự xử lý.
var faces = await faceModel.DetectAsync(mat);

// 4. Lọc theo nghiệp vụ ở tầng ứng dụng (tùy chọn)
var filteredFaces = faces.Where(face => face.Score >= 0.7f).ToList();
```

**Lưu ý:** Đảm bảo rằng runtime EmguCV chính xác đã được cài đặt cho hệ điều hành của bạn.
