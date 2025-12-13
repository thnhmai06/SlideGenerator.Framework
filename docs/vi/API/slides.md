# API Slide

## Template
- `TemplatePresentation` (chỉ bao gồm 1 slide). Lấy slide chính qua `MainSlideRelationshipId` và `GetMainSlidePart()`.
- `DerivedPresentation` sao chép template. `CopySlide(slideRid, position)` nhân bản; `RemoveSlide(position)` xóa; `Save()` lưu.

## Thay thế Văn bản
- Dò placeholder: `TextReplacer.ScanPlaceholders(string|SlidePart)` trả về danh sách token Mustache.
- Thay thế: `TextReplacer.Replace(...)` hoặc `ReplaceAsync(...)` render với `Dictionary<string,string>`.

## Thay thế Hình ảnh
- Truy xuất nội dung: `Presentation.GetPictures(...)`, `GetShapes(...)`, `GetPresentationTexts(...)`, `GetDrawingTexts(...)`.

## Dispose
- Dispose `TemplatePresentation` và `DerivedPresentation` khi dùng xong.
