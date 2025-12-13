# API Link Cloud

## Chuyển đổi
- `CloudUrlResolver.ResolveAsync(url, HttpClient)` đổi link chia sẻ Google Drive/Photos/OneDrive thành link tải trực tiếp nếu có thể.
- `IsCloudStorageUrl(url)` kiểm tra nhanh link có thuộc provider hỗ trợ hay không.

## Exceptions
- Ném `CloudUrlExtractionException` khi không trích xuất được link tải.
