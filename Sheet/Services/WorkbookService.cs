using ClosedXML.Excel;

namespace SlideGenerator.Framework.Sheet.Services;

/// <summary>
///     Provides services for working with workbooks (Excel files).
/// </summary>
/// Reviewed by @thnhmai06 at 05/03/2026
public static class WorkbookService
{
    /// <param name="workbook">The workbook from which to obtain the title. Cannot be null.</param>
    extension(IXLWorkbook workbook)
    {
        /// <summary>
        ///     Retrieves the title of the specified workbook, if one is set.
        /// </summary>
        /// <returns>A string containing the workbook's title if set; otherwise, null.</returns>
        public string? GetName()
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
        /// <returns>
        ///     A read-only dictionary where each key is a worksheet name and each value is the number of data rows in that
        ///     worksheet. If a worksheet contains no data rows, its value will be 0.
        /// </returns>
        public IReadOnlyDictionary<string, int> CountRows()
        {
            var result = new Dictionary<string, int>();
            foreach (var worksheet in workbook.Worksheets)
            {
                var contentRange = worksheet.GetContentRange();

                var name = worksheet.Name;
                var count = Math.Max(contentRange?.RowCount() - 1 ?? 0, 0);
                result[name] = count;
            }

            return result;
        }
    }
}