using System.Collections.Concurrent;
using ClosedXML.Excel;
using SlideGenerator.Framework.Sheet.Contracts;
using SlideGenerator.Framework.Sheet.Exceptions;

namespace SlideGenerator.Framework.Sheet.Models;

/// <summary>
///     Represents a workbook implementation using ClosedXML for Excel file processing.
/// </summary>
public sealed class Workbook : IWorkbook
{
    private readonly XLWorkbook _workbook;
    private readonly ConcurrentDictionary<string, IWorksheet> _worksheets;
    private bool _disposed;

    /// <summary>
    ///     Opens a workbook from the specified file path.
    /// </summary>
    /// <param name="filePath">Path to the Excel file.</param>
    public Workbook(string filePath) : this(new XLWorkbook(filePath), filePath)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the Workbook class using the specified XLWorkbook and file path.
    /// </summary>
    /// <param name="workbook">The XLWorkbook instance to wrap. Cannot be null.</param>
    /// <param name="filePath">The file path associated with the workbook. Defaults to "memory" if not specified.</param>
    public Workbook(XLWorkbook workbook, string filePath = "memory")
    {
        FilePath = filePath;
        _workbook = workbook;
        _worksheets = new ConcurrentDictionary<string, IWorksheet>();

        foreach (var worksheet in _workbook.Worksheets)
            _worksheets[worksheet.Name] = new Worksheet(worksheet);
    }

    /// <inheritdoc />
    public string FilePath { get; }

    /// <inheritdoc />
    public string? Name => _workbook.Properties.Title;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IWorksheet> Worksheets => _worksheets;

    /// <inheritdoc />
    public Dictionary<string, int> GetWorksheetsInfo()
    {
        return _worksheets.ToDictionary(t => t.Key, t => t.Value.RowCount);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _workbook.Dispose();
    }

    /// <summary>
    ///     Gets a worksheet by name.
    /// </summary>
    /// <param name="name">The name of the worksheet.</param>
    /// <returns>The worksheet.</returns>
    /// <exception cref="WorksheetNotFoundException">Thrown when the worksheet is not found.</exception>
    public IWorksheet GetWorksheet(string name)
    {
        return !_worksheets.TryGetValue(name, out var worksheet)
            ? throw new WorksheetNotFoundException(name, FilePath)
            : worksheet;
    }
}