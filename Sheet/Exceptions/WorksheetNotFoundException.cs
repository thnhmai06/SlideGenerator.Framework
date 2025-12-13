namespace SlideGenerator.Framework.Sheet.Exceptions;

/// <summary>
///     Exception thrown when a worksheet is not found in a workbook.
/// </summary>
public class WorksheetNotFoundException(string worksheetName, string? workbookPath = null) : KeyNotFoundException(
    $"Worksheet '{worksheetName}' not found{(workbookPath != null ? $" in workbook '{workbookPath}'" : "")}.")
{
    public string WorksheetName { get; } = worksheetName;
    public string? WorkbookPath { get; } = workbookPath;
}