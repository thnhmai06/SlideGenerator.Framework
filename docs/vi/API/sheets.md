# API Excel

## Workbook
- `new Workbook(path)` tải file XLSX (ClosedXML).
- Truy cập sheet qua dictionary `Worksheets` hoặc `GetWorksheet(name)`; thiếu sheet ném `WorksheetNotFoundException`.
- `GetWorksheetsInfo()` trả về tên sheet -> số dòng.
- Dispose `Workbook` sau khi dùng.

## Worksheet
- Thuộc tính: `Name`, `Headers`, `RowCount`.
- Dữ liệu: `GetRow(rowNumber)` trả dictionary theo header; `GetAllRows()` trả tất cả dòng.
- Có thể đưa dictionary này vào `TextReplacer.Replace(...)` để map placeholder.
