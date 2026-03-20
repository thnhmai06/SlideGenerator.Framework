# Tài liệu Slide Module

[🇬🇧 English Version](../en/slide-module.md)

## Tổng Quan

Slide module cung cấp các trừu tượng cấp cao cho việc thao tác PowerPoint. Nó xử lý việc tải mẫu, sao chép trang trình bày, thay thế văn bản và chèn hình ảnh—các hoạt động cốt lõi cần thiết để tạo trang trình bày theo chương trình.

## Kiến Trúc

### Các Thành Phần Chính

#### TemplatePresentation

Đại diện cho một tệp mẫu PowerPoint (chỉ đọc):

```csharp
namespace SlideGenerator.Framework.Features.Slide.Models;

public sealed class TemplatePresentation : IDisposable
{
    /// <summary>
    ///     Tải mẫu trang trình bày từ đường dẫn tệp được chỉ định.
    /// </summary>
    /// <param name="path">Đường dẫn đến tệp mẫu .pptx.</param>
    public TemplatePresentation(string path);
    
    /// <summary>
    ///     Lấy ID quan hệ của trang trình bày chính (trang 1).
    /// </summary>
    public string MainSlideRelationshipId { get; }
    
    /// <summary>
    ///     Lấy tất cả các hình ảnh/hình ảnh trong mẫu.
    /// </summary>
    /// <returns>Danh sách ID hình dạng chứa hình ảnh.</returns>
    public List<uint> GetAllPreviewImageShapes();
    
    /// <summary>
    ///     Lấy tất cả các hình dạng giữ chỗ trong mẫu.
    /// </summary>
    /// <returns>Danh sách ID hình dạng cho các giữ chỗ văn bản.</returns>
    public List<uint> GetAllPlaceholders();
    
    public void Dispose();
}
```

#### WorkingPresentation

Đại diện cho tệp PowerPoint đầu ra (đọc-ghi):

```csharp
public sealed class WorkingPresentation : IDisposable
{
    /// <summary>
    ///     Lấy PresentationDocument nội bộ cho các hoạt động nâng cao.
    /// </summary>
    public PresentationDocument Document { get; }
    
    /// <summary>
    ///     Sao chép trang trình bày từ mẫu.
    /// </summary>
    /// <param name="templateRelationshipId">ID trang trình bày để sao chép từ.</param>
    /// <returns>Phần trang trình bày của trang đã sao chép.</returns>
    public SlidePart CopySlide(string templateRelationshipId);
    
    /// <summary>
    ///     Lưu trang trình bày vào tệp.
    /// </summary>
    public void Save();
    
    public void Dispose();
}
```

### Thay Thế Văn Bản

Thay thế văn bản giữ chỗ được đánh dấu bằng cú pháp `{{Khóa}}`:

```csharp
namespace SlideGenerator.Framework.Features.Slide.Services.Replacer;

public static class TextReplacer
{
    /// <summary>
    ///     Thay thế tất cả các giữ chỗ văn bản trong một trang trình bày.
    /// </summary>
    /// <param name="slidePart">Trang trình bày để tìm kiếm và thay thế.</param>
    /// <param name="replacements">Từ điển khóa → giá trị để thay thế.</param>
    public static Task ReplaceAsync(
        SlidePart slidePart,
        IReadOnlyDictionary<string, string> replacements);
}
```

### Thay Thế Hình Ảnh

Thay thế hình ảnh theo ID hình dạng:

```csharp
public static class ImageReplacer
{
    /// <summary>
    ///     Thay thế một hình ảnh trong một hình dạng cụ thể.
    /// </summary>
    /// <param name="slidePart">Trang trình bày chứa hình dạng hình ảnh.</param>
    /// <param name="shape">Đối tượng hình dạng chứa hình ảnh.</param>
    /// <param name="imageStream">Luồng hình ảnh mới (PNG, JPG, v.v.).</param>
    public static void ReplaceImage(
        SlidePart slidePart,
        Shape shape,
        Stream imageStream);
}
```

### Tìm Hình Dạng

Tiện ích để định vị các hình dạng trong các trang trình bày:

```csharp
public static class ShapeService
{
    /// <summary>
    ///     Tìm một hình dạng theo ID duy nhất của nó.
    /// </summary>
    /// <param name="slidePart">Trang trình bày để tìm kiếm.</param>
    /// <param name="shapeId">ID hình dạng.</param>
    /// <returns>Hình dạng, hoặc null nếu không tìm thấy.</returns>
    public static Shape? FindShapeById(SlidePart slidePart, uint shapeId);
    
    /// <summary>
    ///     Tìm một hình ảnh theo ID.
    /// </summary>
    /// <param name="slidePart">Trang trình bày để tìm kiếm.</param>
    /// <param name="shapeId">ID hình dạng.</param>
    /// <returns>Hình ảnh, hoặc null nếu không tìm thấy.</returns>
    public static Picture? FindPictureById(SlidePart slidePart, uint shapeId);
}
```

## Cách Sử Dụng

### Luồng Tạo Cơ Bản

```csharp
using SlideGenerator.Framework.Features.Slide.Models;
using SlideGenerator.Framework.Features.Slide.Services;

public class SlideGenerator
{
    public void GeneratePresentation(
        string templatePath,
        string outputPath,
        Dictionary<string, string> textReplacements,
        Dictionary<uint, string> imageReplacements)
    {
        // 1. Tải mẫu
        using var template = new TemplatePresentation(templatePath);
        
        // 2. Tạo đầu ra từ mẫu
        using var working = template.SaveAs(outputPath);
        
        // 3. Sao chép trang trình bày
        var slidePart = working.CopySlide(template.MainSlideRelationshipId);
        
        // 4. Thay thế văn bản
        await TextReplacer.ReplaceAsync(slidePart, textReplacements);
        
        // 5. Thay thế hình ảnh
        foreach (var (shapeId, imagePath) in imageReplacements)
        {
            var shape = ShapeService.FindShapeById(slidePart, shapeId);
            if (shape != null)
            {
                using var imgStream = File.OpenRead(imagePath);
                ImageReplacer.ReplaceImage(slidePart, shape, imgStream);
            }
        }
        
        // 6. Lưu
        working.Save();
    }
}
```

### Hướng Dẫn Từng Bước

#### 1. Tải Mẫu

```csharp
using var template = new TemplatePresentation("template.pptx");

// Khám phá những gì có thể được sửa đổi
var imageShapes = template.GetAllPreviewImageShapes();
var placeholders = template.GetAllPlaceholders();
```

#### 2. Tạo Trang Trình Bày Làm Việc

```csharp
using var working = template.SaveAs("output.pptx");
// Tạo một bản sao để sửa đổi
```

#### 3. Sao Chép Trang Trình Bày

```csharp
var slideId = template.MainSlideRelationshipId;
var slidePart = working.CopySlide(slideId);
```

#### 4. Thay Thế Giữ Chỗ Văn Bản

```csharp
var replacements = new Dictionary<string, string>
{
    ["{{Tên}}"] = "Nguyễn Văn A",
    ["{{Chức Vụ}}"] = "Kỹ Sư Cao Cấp",
    ["{{Ngày}}"] = DateTime.Now.ToString("yyyy-MM-dd")
};

await TextReplacer.ReplaceAsync(slidePart, replacements);
```

#### 5. Thay Thế Hình Ảnh

```csharp
var pictureShape = ShapeService.FindPictureById(slidePart, 4);
if (pictureShape != null)
{
    using var imageStream = File.OpenRead("photo.jpg");
    ImageReplacer.ReplaceImage(slidePart, pictureShape, imageStream);
}
```

#### 6. Lưu Trang Trình Bày

```csharp
working.Save();
// Output.pptx sẵn sàng với tất cả các thay đổi
```

## Thiết Kế Mẫu

### Giữ Chỗ Văn Bản

Sử dụng cú pháp `{{Khóa}}` trong hộp văn bản:

```
Văn bản mẫu: "Xin chào {{Tên}}"
Thay thế:   {"Tên": "Nguyễn Văn A"}
Kết quả:    "Xin chào Nguyễn Văn A"
```

**Quy tắc:**
- Khớp phân biệt chữ hoa/thường
- Hỗ trợ nhiều giữ chỗ mỗi hộp văn bản
- ✅ `"Tên: {{Tên}}, Chức Vụ: {{ChứcVụ}}"` → Tất cả thay thế
- ❌ Mẫu Regex: Không được hỗ trợ

### Giữ Chỗ Hình Ảnh

Đánh dấu hình ảnh với ID Hình Dạng:

1. Trong mẫu PowerPoint, chèn hình ảnh giữ chỗ
2. Nhấp chuột phải → Định Dạng Hình Ảnh → Tên/ID
3. Ghi chú ID hình dạng
4. Sử dụng `ImageReplacer` với ID đó

## Xem Xét Hiệu Suất

### Chi Phí Sao Chép

- **Sao chép đầu tiên**: ~50-200ms
- **Sao chép tiếp theo**: ~30-100ms
- **Tối ưu hóa**: Sao chép một lần, tái sử dụng nếu có thể

### Sử Dụng Bộ Nhớ

- Mẫu: ~1-10MB
- Bản sao làm việc: ~2-20MB
- **Tối ưu hóa**: Giải phóng workbooks sau mỗi lần tạo

## Thực Hành Tốt Nhất

### 1. Xác Thực Mẫu

```csharp
public void ValidateTemplate(string templatePath)
{
    using var template = new TemplatePresentation(templatePath);
    
    var imageShapes = template.GetAllPreviewImageShapes();
    var placeholders = template.GetAllPlaceholders();
    
    if (imageShapes.Count == 0)
        throw new InvalidOperationException("Mẫu không có giữ chỗ hình ảnh");
}
```

### 2. Dọn Dẹp Tài Nguyên

```csharp
// ✅ Tốt: Using statements
using var template = new TemplatePresentation(templatePath);
using var working = template.SaveAs(outputPath);
{
    // Làm việc với trang trình bày
}
// Tất cả tài nguyên được dọn dẹp

// ❌ Xấu: Không dọn dẹp
var template = new TemplatePresentation(templatePath);
var working = template.SaveAs(outputPath);
// Tay cầm tệp không được giải phóng
```

## Hạn Chế

- ❌ Nhiều trang trình bày mẫu: Chỉ trang 1 được sao chép
- ❌ Hiệu ứng phức tạp: Được bảo toàn nhưng có thể không hoạt động như dự kiến
- ❌ Macro VBA: Bị loại bỏ khi lưu
- ✅ Định dạng văn bản: Được bảo toàn khi thay thế
- ✅ Định vị hình ảnh: Được bảo toàn khi thay thế
- ✅ Hình dạng phức tạp: Được hỗ trợ

## An Toàn Luồng

- ❌ `TemplatePresentation` và `WorkingPresentation` **KHÔNG an toàn luồng**
- ✅ Tạo các instance riêng cho mỗi luồng
- ✅ Nhiều luồng có thể làm việc trên các trang trình bày khác nhau đồng thời

---

Tiếp theo: [Image Module](image-module.md) | [Tổng Quan](overview.md)

