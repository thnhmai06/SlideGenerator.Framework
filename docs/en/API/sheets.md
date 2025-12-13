# Sheets API

## Workbook
- `new Workbook(path)` loads an XLSX file using ClosedXML.
- Access sheets via `Worksheets` dictionary or `GetWorksheet(name)`; missing sheet throws `WorksheetNotFoundException`.
- `GetWorksheetsInfo()` returns sheet name -> row count.
- Dispose `Workbook` when done.

## Worksheet
- Properties: `Name`, `Headers`, `RowCount`.
- Data access: `GetRow(rowNumber)` returns a header-keyed dictionary; `GetAllRows()` returns all rows.
- Pair these dictionaries with `TextReplacer.Replace(...)` for slide templating.
