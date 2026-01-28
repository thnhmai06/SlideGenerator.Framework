using ClosedXML.Excel;

namespace SlideGenerator.Framework.Sheet;

/// <summary>
///     Provides services for working with workbooks (Excel files).
/// </summary>
public static class WorkbookService
{
    /// <summary>
    /// Opens an Excel workbook from the specified file path.
    /// </summary>
    /// <remarks>The caller is responsible for disposing the returned <see cref="IXLWorkbook"/> when it is no longer needed.
    /// If the file is opened in read-only mode, changes to the workbook will not be saved to the original
    /// file.</remarks>
    /// <param name="filePath">The full path to the Excel file to open. The file must exist and be accessible.</param>
    /// <param name="readOnly">true to open the workbook in read-only mode; otherwise, false. The default is true.</param>
    /// <returns>An <see cref="IXLWorkbook"/> instance representing the opened Excel workbook.</returns>
    public static IXLWorkbook OpenWorkbook(string filePath, bool readOnly = true)
    {
        using var fs = new FileStream(
            filePath, FileMode.Open, 
            readOnly ? FileAccess.ReadWrite : FileAccess.Read, 
            readOnly ? FileShare.ReadWrite : FileShare.Read);
        var workbook = new XLWorkbook(fs);
        return workbook;
    }

    /// <summary>
    ///     Retrieves the title of the specified workbook, if one is set.
    /// </summary>
    /// <param name="workbook">The workbook from which to obtain the title. Cannot be null.</param>
    /// <returns>A string containing the workbook's title if set; otherwise, null.</returns>
    public static string? GetName(IXLWorkbook workbook)
    {
        return workbook.Properties.Title;
    }

    /// <summary>
    ///     Returns a read-only dictionary containing the number of data rows for each worksheet in the specified workbook.
    /// </summary>
    /// <remarks>
    ///     The row count for each worksheet excludes the header row and only counts rows within the
    ///     content range as determined by WorksheetService.GetContentRange. Worksheets with no content will have a row
    ///     count of 0.
    /// </remarks>
    /// <param name="workbook">The workbook from which to retrieve the row counts for each worksheet. Cannot be null.</param>
    /// <returns>
    ///     A read-only dictionary where each key is a worksheet name and each value is the number of data rows in that
    ///     worksheet. If a worksheet contains no data rows, its value will be 0.
    /// </returns>
    public static IReadOnlyDictionary<string, int> GetSheetsRowCount(IXLWorkbook workbook)
    {
        var result = new Dictionary<string, int>();
        foreach (var worksheet in workbook.Worksheets)
        {
            var contentRange = WorksheetService.GetContentRange(worksheet);

            var name = worksheet.Name;
            var count = Math.Max(contentRange?.RowCount() - 1 ?? 0, 0);
            result[name] = count;
        }

        return result;
    }
}