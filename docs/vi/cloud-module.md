# Tài liệu Cloud Module

[🇬🇧 English Version](../en/cloud-module.md)

## Tổng Quan

Cloud module giải quyết việc chuyển đổi các liên kết chia sẻ từ các nền tảng lưu trữ đám mây (Google Drive, OneDrive, Google Photos) thành các URL tải trực tiếp. Điều này cho phép Framework truy cập các hình ảnh được lưu trữ trên đám mây mà không cần tải xuống thủ công hoặc quản lý xác thực.

## Các Nền Tảng Được Hỗ Trợ

| Nền Tảng | Định Dạng Liên Kết | Ví Dụ |
|----------|------------------|-------|
| **Google Drive** | `https://drive.google.com/file/d/{ID}/view` | Hỗ trợ chia sẻ trực tiếp |
| **OneDrive** | `https://onedrive.live.com/...` | Hỗ trợ liên kết chia sẻ |
| **Google Photos** | `https://photos.app.goo.gl/{ID}` | Hỗ trợ album/ảnh chia sẻ |

## Kiến Trúc

### CloudUrlResolver

Lớp tiện ích tĩnh để giải quyết các liên kết đám mây:

```csharp
namespace SlideGenerator.Framework.Features.Cloud.Services;

public static class CloudUrlResolver
{
    /// <summary>
    ///     Giải quyết liên kết chia sẻ từ đám mây thành URL tải trực tiếp.
    /// </summary>
    /// <param name="shareLink">URL chia sẻ từ nền tảng đám mây.</param>
    /// <returns>URL tải trực tiếp có thể sử dụng với HttpClient.</returns>
    public static Task<string> ResolveLinkAsync(string shareLink);
}
```

## Cách Sử Dụng

### Giải Quyết Liên Kết Cơ Bản

```csharp
using SlideGenerator.Framework.Features.Cloud.Services;

public class ImageDownloader
{
    public async Task<Stream> DownloadFromCloudAsync(string shareLink)
    {
        // Giải quyết liên kết chia sẻ thành URL trực tiếp
        var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
        
        // Sử dụng HttpClient để tải xuống
        using var client = new HttpClient();
        return await client.GetStreamAsync(directUrl);
    }
}
```

### Ví Dụ Google Drive

```csharp
// Đầu vào: https://drive.google.com/file/d/1ABC123DEF456/view
var shareLink = "https://drive.google.com/file/d/1ABC123DEF456/view";
var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
// Đầu ra: https://drive.google.com/uc?export=download&id=1ABC123DEF456
```

### Ví Dụ OneDrive

```csharp
// Đầu vào: https://onedrive.live.com/?authkey=...&cid=...&id=...&parId=...
var shareLink = "https://onedrive.live.com/?authkey=...";
var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
// Đầu ra: URL tải trực tiếp với mã truy cập
```

## Chi Tiết Triển Khai

### Giải Quyết Google Drive

Bộ phân giải trích xuất ID tệp từ liên kết chia sẻ Google Drive và xây dựng URL tải trực tiếp:

```
Đầu vào:  https://drive.google.com/file/d/{FILE_ID}/view
Đầu ra: https://drive.google.com/uc?export=download&id={FILE_ID}
```

**Tính năng:**
- Hỗ trợ cả điểm cuối `/view` và `/preview`
- Xử lý tham số URL và fragment
- Không cần xác thực cho các chia sẻ công khai

### Giải Quyết OneDrive

Các chia sẻ OneDrive yêu cầu phân tích mã truy cập nhúng và xây dựng điểm cuối tải:

```
Đầu vào:  https://onedrive.live.com/?authkey={AUTHKEY}&cid={CID}&id={ITEM_ID}&parId={PAR_ID}
Đầu ra: https://onedrive.live.com/download?resauth={RESAUTH}&authkey={AUTHKEY}&cid={CID}&id={ITEM_ID}
```

### Giải Quyết Google Photos

Các liên kết ngắn Google Photos được mở rộng thành URL hình ảnh độ phân giải đầy đủ.

## Xử Lý Lỗi

```csharp
try
{
    var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
}
catch (ArgumentException ex)
{
    // Định dạng URL không hợp lệ
    // Kiểm tra: URL hợp lệ, nền tảng được hỗ trợ
}
catch (HttpRequestException ex)
{
    // Lỗi mạng hoặc chia sẻ không hợp lệ
    // Kiểm tra: Liên kết công khai, kết nối mạng
}
```

## Hiệu Suất

- **Thời gian giải quyết**: ~100-500ms mỗi liên kết (tùy thuộc vào nền tảng)
- **Gọi mạng**: 1 yêu cầu HTTP cho nền tảng đám mây
- **Bộ nhớ cache**: URL đã giải quyết nên được lưu trong bộ nhớ cache phía máy khách nếu truy cập cùng liên kết nhiều lần

## Thực Hành Tốt Nhất

1. **Bộ nhớ cache URL đã giải quyết**: Các nền tảng đám mây có thể giới hạn tốc độ tra cứu lặp lại
2. **Xác thực liên kết chia sẻ**: Kiểm tra định dạng URL trước khi giải quyết
3. **Xử lý lỗi mạng**: Giải quyết đám mây có thể thất bại tạm thời

## Hạn Chế

- ❌ Các chia sẻ riêng/bị hạn chế yêu cầu xác thực
- ❌ Liên kết chia sẻ hết hạn sẽ thất bại
- ❌ Có thể áp dụng giới hạn tốc độ cho các yêu cầu khối lượng cao
- ✅ Các chia sẻ công khai từ các nền tảng được hỗ trợ hoạt động ngay lập tức
- ✅ Không cần khóa API hoặc mã thông báo xác thực cho các chia sẻ công khai

## An Toàn Luồng

`CloudUrlResolver` là an toàn luồng và có thể được gọi từ nhiều luồng đồng thời.

---

Tiếp theo: [Sheet Module](sheet-module.md) | [Tổng Quan](overview.md)

