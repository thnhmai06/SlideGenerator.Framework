using ClosedXML.Excel;
using SlideGenerator.Framework.Sheet.Contracts;
using RowContent = System.Collections.Generic.Dictionary<string, string?>;

namespace SlideGenerator.Framework.Sheet.Models;

/// <summary>
///     Represents a worksheet implementation using ClosedXML.
/// </summary>
internal sealed class Worksheet : IWorksheet
{
    private readonly List<string?> _headers = [];
    private readonly int _maxCol;
    private readonly int _maxRow;
    private readonly int _minCol;
    private readonly int _minRow;
    private readonly IXLWorksheet _worksheet;

    internal Worksheet(IXLWorksheet worksheet)
    {
        _worksheet = worksheet;

        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
        {
            _minRow = _maxRow = _minCol = _maxCol = 1;
        }
        else
        {
            _minRow = usedRange.FirstRow().RowNumber();
            _maxRow = usedRange.LastRow().RowNumber();
            _minCol = usedRange.FirstColumn().ColumnNumber();
            _maxCol = usedRange.LastColumn().ColumnNumber();

            for (var col = _minCol; col <= _maxCol; col++)
            {
                var cellValue = worksheet.Cell(_minRow, col).GetValue<string>();
                _headers.Add(cellValue);
            }
        }
    }

    public string Name => _worksheet.Name;
    public IReadOnlyList<string?> Headers => _headers;
    public int RowCount => _maxRow - _minRow;

    /// <summary>
    ///     Retrieves the values of all columns in the specified row as a dictionary mapping column headers to cell values.
    /// </summary>
    /// <remarks>
    ///     The returned dictionary includes all columns in the row. If a column header is missing, a
    ///     default name in the format "ColumnN" is used, where N is the column number.
    /// </remarks>
    /// <param name="rowNumber">
    ///     The one-based index of the row to retrieve. Must be between 1 and the total number of rows,
    ///     inclusive.
    /// </param>
    /// <returns>
    ///     A dictionary containing the column headers as keys and the corresponding cell values for the specified row. The
    ///     value is <see langword="null" /> if a cell is empty.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     Thrown when <paramref name="rowNumber" /> is less than 1 or greater than the
    ///     total number of rows.
    /// </exception>
    public RowContent GetRow(int rowNumber)
    {
        if (rowNumber < 1 || rowNumber > RowCount)
            throw new IndexOutOfRangeException($"Row index {rowNumber} is out of range [1, {RowCount}].");

        var actualRow = _minRow + rowNumber;
        var rowData = new RowContent();

        for (var col = _minCol; col <= _maxCol; col++)
        {
            var header = _headers[col - _minCol];
            var cellValue = _worksheet.Cell(actualRow, col).Value;
            var value = cellValue.ToString();
            rowData[header ?? $"Column{col}"] = value;
        }

        return rowData;
    }

    /// <summary>
    ///     Retrieves all rows in the collection as a list of dictionaries, where each dictionary represents a single row
    ///     with column names as keys.
    /// </summary>
    /// <remarks>
    ///     The order of the rows in the returned list corresponds to their original order in the
    ///     collection. Modifying the returned dictionaries does not affect the underlying data source.
    /// </remarks>
    /// <returns>
    ///     A list of dictionaries containing all rows. Each dictionary maps column names to their corresponding string
    ///     values; values may be null if the cell is empty.
    /// </returns>
    public List<RowContent> GetAllRows()
    {
        var rows = new List<RowContent>();
        for (var i = 1; i <= RowCount; i++)
            rows.Add(GetRow(i));

        return rows;
    }
}