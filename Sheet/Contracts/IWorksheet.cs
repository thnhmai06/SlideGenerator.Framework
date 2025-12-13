namespace SlideGenerator.Framework.Sheet.Contracts;

/// <summary>
///     Represents a worksheet within a workbook.
/// </summary>
public interface IWorksheet
{
    /// <summary>
    ///     Gets the name of the worksheet.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the column headers from the first row.
    /// </summary>
    IReadOnlyList<string?> Headers { get; }

    /// <summary>
    ///     Gets the number of data rows (excluding header row).
    /// </summary>
    int RowCount { get; }

    /// <summary>
    ///     Gets a specific row by row number (1-based, relative to data rows after header).
    /// </summary>
    /// <param name="rowNumber">The row number (1-based).</param>
    /// <returns>A dictionary mapping column headers to cell values.</returns>
    Dictionary<string, string?> GetRow(int rowNumber);

    /// <summary>
    ///     Gets all data rows from the worksheet.
    /// </summary>
    /// <returns>A list of dictionaries, each representing a row.</returns>
    List<Dictionary<string, string?>> GetAllRows();
}