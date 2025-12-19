namespace SlideGenerator.Framework.Sheet.Contracts;

/// <summary>
///     Represents a workbook containing multiple worksheets.
/// </summary>
public interface IWorkbook : IDisposable
{
    /// <summary>
    ///     Gets the file path of the workbook.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    ///     Gets the title/name of the workbook, if available.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets all worksheets in the workbook.
    /// </summary>
    IReadOnlyDictionary<string, IWorksheet> Worksheets { get; }

    /// <summary>
    ///     Gets information about all worksheets (name and row count).
    /// </summary>
    /// <returns>A dictionary mapping worksheet names to row counts.</returns>
    IReadOnlyDictionary<string, int> GetWorksheetsInfo();
}