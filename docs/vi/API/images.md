# API Ảnh

## Nạp và lưu
- `new ImageData(path)` hoặc `new ImageData(byte[])` để nạp vào OpenCV `Mat`.
- `ImageData.Save(path?)` lưu PNG; `ToByteArray()` trả về bytes PNG.

## Cắt và ROI
- `ImageProcessor.GetRoi(image, RoiType, Size)` lấy rectangle cho kích thước đích.
- `ImageProcessor.Crop(image, roi)` cắt tại chỗ; `CropToRoiCopy(...)` tạo ảnh mới.
- Các chế độ ROI:
  - `RoiType.Prominent` - phân tích spectral residual để tìm vùng nổi bật nhất
  - `RoiType.Center` - cắt đơn giản ở giữa
  - `RoiType.Attention` - cắt thông minh kết hợp nhận diện khuôn mặt với độ nổi bật cho ảnh người

## Tùy chọn ROI
- `RoiOptions` cấu hình nhận diện khuôn mặt và padding cho `RoiType.Attention`:
  - `FaceConfidence` (mặc định: 0.6) - ngưỡng độ tin cậy tối thiểu (0-1) để chấp nhận khuôn mặt
  - `FacePaddingRatio` - padding xung quanh khuôn mặt, có thể cấu hình riêng theo 4 hướng:
    - Constructor: `new ExpandRatio(top, bottom, left, right)` - padding riêng từng hướng
    - Constructor: `new ExpandRatio(vertical, horizontal)` - padding dọc/ngang
    - Constructor: `new ExpandRatio(allSides)` - padding đồng nhất
  - `FacesUnionAll` (mặc định: true) - nếu true, hợp tất cả khuôn mặt; nếu false, chọn khuôn mặt tốt nhất
  - `SaliencyPaddingRatio` - padding xung quanh vùng nổi bật, cũng hỗ trợ 4 hướng như trên
- Tạo `ImageProcessor` với tùy chọn: `new ImageProcessor(roiOptions)`
- Nhận diện khuôn mặt yêu cầu khởi tạo: `await processor.InitFaceModelAsync()`

## Ví dụ

### Padding đồng nhất
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(0.15f)  // 15% tất cả các hướng
};
```

### Padding theo dọc/ngang
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(
        vertical: 0.20f,    // 20% trên và dưới
        horizontal: 0.15f   // 15% trái và phải
    )
};
```

### Padding riêng từng hướng
```csharp
var options = new RoiOptions 
{ 
    FacePaddingRatio = new ExpandRatio(
        top: 0.25f,     // 25% phía trên
        bottom: 0.15f,  // 15% phía dưới
        left: 0.20f,    // 20% bên trái
        right: 0.20f    // 20% bên phải
    )
};
```

## Dùng trong slide
- Lấy kích thước Shapes: `ImageReplacer.GetPictureSize(...)` hoặc `GetShapeSize(...)` (xem [Slides](slides.md)).
- Thay thế ảnh: `ImageReplacer.ReplaceImage(SlidePart, Picture|Shape, Stream)` với stream PNG.

## Dispose
- Dispose `ImageData` để giải phóng bộ nhớ native.
- Dispose `ImageProcessor` để giải phóng tài nguyên mô hình nhận diện khuôn mặt.
