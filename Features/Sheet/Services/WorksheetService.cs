using ClosedXML.Excel;

namespace SlideGenerator.Framework.Sheet;

/// <summary>
///     Provides services for working with Excel worksheets.
/// </summary>
public static class WorksheetService
{
    /// <summary>
    ///     Returns the range of cells on the specified worksheet that contain content.
    /// </summary>
    /// <remarks>
    ///     Content is defined as cells that are not empty, including those with formulas, values, or
    ///     text. The returned range may be discontinuous if content is scattered across the worksheet.
    /// </remarks>
    /// <param name="worksheet">The worksheet to search for cells containing content. Cannot be null.</param>
    /// <returns>
    ///     An <see cref="IXLRange" /> representing the range of cells with content, or <see langword="null" /> if no cells
    ///     contain content.
    /// </returns>
    public static IXLRange? GetContentRange(IXLWorksheet worksheet)
    {
        return worksheet.RangeUsed(XLCellsUsedOptions.Contents);
    }

    /// <summary>
    ///     Retrieves the contents of a specified row from the given range, mapping each header to its corresponding cell
    ///     value.
    /// </summary>
    /// <remarks>
    ///     The method assumes that the first row of the range contains headers and that each subsequent
    ///     row contains data. If a header cell is empty, its column is excluded from the result. The returned dictionary
    ///     does not include duplicate header names.
    /// </remarks>
    /// <param name="contentRange">
    ///     The range containing the table data, where the first row is expected to contain column
    ///     headers.
    /// </param>
    /// <param name="rowIndex">
    ///     The 1-based index of the data row to retrieve, excluding the header row. Must be greater than or equal to 0
    ///     and less than the number of data rows in the range.
    /// </param>
    /// <returns>
    ///     An immutable dictionary mapping each non-empty header name to its corresponding cell value from the specified
    ///     row. If a header is duplicated, only the first occurrence is included.
    /// </returns>
    public static IReadOnlyDictionary<string, string> GetRowContent(IXLRange contentRange, int rowIndex)
    {
        var headerCells = contentRange.FirstRow().Cells();
        var dataCells = contentRange.Row(rowIndex + 1).Cells();

        return headerCells
            .Zip(dataCells, (header, cell) => new
            {
                Key = header.GetString(),
                Value = cell.GetString()
            })
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }
}