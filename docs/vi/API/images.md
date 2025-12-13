# API Ảnh

## Nạp và lưu
- `new ImageData(path)` hoặc `new ImageData(byte[])` để nạp vào OpenCV `Mat`.
- `ImageData.Save(path?)` lưu PNG; `ToByteArray()` trả về bytes PNG.

## Cắt và ROI
- `ImageProcessor.GetRoi(image, RoiType, Size)` lấy rectangle cho kích thước đích.
- `ImageProcessor.Crop(image, roi)` cắt tại chỗ; `CropToRoiCopy(...)` tạo ảnh mới.
- ROI: `RoiType.Prominent` (vùng nổi bật) hoặc `RoiType.Center`.

## Dùng trong slide
- Lấy kích thước mục tiêu: `ImageReplacer.GetPictureSize(...)` hoặc `GetShapeSize(...)` (xem [api-slides.vi.md](api-slides.vi.md)).
- Thay dữ liệu ảnh: `ImageReplacer.ReplaceImage(SlidePart, Picture|Shape, Stream)` với stream PNG.

## Dispose
- Dispose `ImageData` để giải phóng bộ nhớ native.
