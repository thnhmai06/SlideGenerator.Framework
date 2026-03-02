# Sheet Module Documentation

[🇻🇳 Vietnamese Version](../vi/sheet-module.md)

## Overview

The Sheet module provides a lightweight, efficient API for reading data from Excel files (.xlsx) and CSV sources. It abstracts the complexity of OpenXML while maintaining flexibility for different data extraction patterns.

## Architecture

### Core Components

#### Workbook

Represents an Excel file (.xlsx):

```csharp
namespace SlideGenerator.Framework.Features.Sheet.Models;

public sealed class Workbook : IDisposable
{
    /// <summary>
    ///     Gets the collection of worksheets in this workbook.
    /// </summary>
    public ICollection<Worksheet> Worksheets { get; }
    
    /// <summary>
    ///     Gets or creates a worksheet by name.
    /// </summary>
    /// <param name="name">The worksheet name (case-insensitive).</param>
    /// <returns>The worksheet, or null if not found.</returns>
    public Worksheet? GetWorksheet(string name);
    
    /// <summary>
    ///     Gets or creates a worksheet by index (0-based).
    /// </summary>
    /// <param name="index">The worksheet index.</param>
    /// <returns>The worksheet, or null if index out of range.</returns>
    public Worksheet? GetWorksheet(int index);
    
    /// <summary>
    ///     Disposes the workbook and releases file handles.
    /// </summary>
    public void Dispose();
}
```

#### Worksheet

Represents a sheet within a workbook:

```csharp
public sealed class Worksheet
{
    /// <summary>
    ///     Gets the name of this worksheet.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///     Gets the number of rows with data.
    /// </summary>
    public int RowCount { get; }
    
    /// <summary>
    ///     Gets the header row (first row with data).
    /// </summary>
    public IReadOnlyList<string> Headers { get; }
    
    /// <summary>
    ///     Gets a specific row as a dictionary of column name → value.
    /// </summary>
    /// <param name="rowIndex">0-based row index (0 = headers, 1+ = data rows).</param>
    /// <returns>Dictionary with column names as keys, cell values as values.</returns>
    public Dictionary<string, object> GetRow(int rowIndex);
    
    /// <summary>
    ///     Gets all data rows (excluding header).
    /// </summary>
    /// <returns>Collection of row dictionaries.</returns>
    public IEnumerable<Dictionary<string, object>> GetAllRows();
    
    /// <summary>
    ///     Gets a specific cell value.
    /// </summary>
    /// <param name="row">0-based row index.</param>
    /// <param name="column">0-based column index.</param>
    /// <returns>Cell value as object (string, number, date, etc.)</returns>
    public object? GetCell(int row, int column);
}
```

## Usage

### Loading a Workbook

```csharp
using SlideGenerator.Framework.Features.Sheet.Models;

// Load from file
using var workbook = new Workbook("data.xlsx");

// Access worksheet by name
var sheet = workbook.GetWorksheet("Employees");
if (sheet == null)
    throw new InvalidOperationException("Sheet 'Employees' not found");
```

### Reading Data

#### Row-by-Row Access

```csharp
using var workbook = new Workbook("data.xlsx");
var sheet = workbook.GetWorksheet("Employees");

// Get headers
Console.WriteLine($"Columns: {string.Join(", ", sheet.Headers)}");
// Output: Columns: Name, Title, Department, Email

// Read specific row
var row1 = sheet.GetRow(1); // First data row
Console.WriteLine($"Name: {row1["Name"]}");
Console.WriteLine($"Title: {row1["Title"]}");
```

#### All Rows at Once

```csharp
var sheet = workbook.GetWorksheet("Employees");

foreach (var row in sheet.GetAllRows())
{
    var name = row["Name"];
    var title = row["Title"];
    var department = row["Department"];
    
    Console.WriteLine($"{name} - {title} ({department})");
}
```

#### Direct Cell Access

```csharp
var sheet = workbook.GetWorksheet("Employees");

// Get cell by position (0-based)
var cell = sheet.GetCell(1, 0); // Row 1, Column 0
Console.WriteLine($"Value: {cell}");
```

### Data Type Handling

Excel cells are returned as objects with appropriate types:

```csharp
var row = sheet.GetRow(1);

// String values
var name = (string)row["Name"];

// Numeric values
var salary = Convert.ToDouble(row["Salary"]);

// Date values
var hireDate = (DateTime)row["HireDate"];

// Null/empty cells
if (row["OptionalField"] == null)
    Console.WriteLine("Cell is empty");
```

### Error Handling

```csharp
try
{
    using var workbook = new Workbook("data.xlsx");
    var sheet = workbook.GetWorksheet("NonExistent");
    
    if (sheet == null)
        throw new InvalidOperationException("Sheet not found");
    
    var row = sheet.GetRow(999); // May be out of range
}
catch (FileNotFoundException)
{
    // File doesn't exist
}
catch (InvalidOperationException)
{
    // Invalid Excel format
}
```

## Common Patterns

### Template-Based Generation

```csharp
public class SlideGenerator
{
    public async Task GenerateFromExcel(string templatePath, string dataPath)
    {
        using var workbook = new Workbook(dataPath);
        var sheet = workbook.GetWorksheet("Data");
        
        foreach (var row in sheet.GetAllRows())
        {
            // Use row data to clone and populate slides
            await GenerateSlide(templatePath, row);
        }
    }
    
    private async Task GenerateSlide(string templatePath, Dictionary<string, object> rowData)
    {
        // Extract values
        var name = rowData["Name"];
        var description = rowData["Description"];
        var imageUrl = rowData["ImageUrl"];
        
        // Generate slide with data
        // ...
    }
}
```

### Data Validation

```csharp
public class DataValidator
{
    public bool ValidateSheet(Worksheet sheet, string[] requiredColumns)
    {
        var headers = new HashSet<string>(sheet.Headers, StringComparer.OrdinalIgnoreCase);
        
        foreach (var column in requiredColumns)
        {
            if (!headers.Contains(column))
            {
                Console.WriteLine($"Missing required column: {column}");
                return false;
            }
        }
        
        return true;
    }
    
    public void ValidateRows(Worksheet sheet)
    {
        var rowIndex = 1;
        foreach (var row in sheet.GetAllRows())
        {
            // Check for empty critical fields
            if (string.IsNullOrWhiteSpace(row["Name"]?.ToString()))
                Console.WriteLine($"Row {rowIndex}: Name is empty");
            
            rowIndex++;
        }
    }
}
```

### Data Transformation

```csharp
public class DataTransformer
{
    public List<T> ConvertToObjects<T>(Worksheet sheet) where T : new()
    {
        var results = new List<T>();
        
        foreach (var row in sheet.GetAllRows())
        {
            var obj = new T();
            MapRowToObject(obj, row);
            results.Add(obj);
        }
        
        return results;
    }
    
    private void MapRowToObject<T>(T obj, Dictionary<string, object> row)
    {
        var properties = typeof(T).GetProperties();
        
        foreach (var prop in properties)
        {
            if (row.TryGetValue(prop.Name, out var value) && value != null)
            {
                try
                {
                    prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                }
                catch (InvalidCastException)
                {
                    // Value type mismatch
                }
            }
        }
    }
}
```

## Performance Considerations

### Lazy Loading

Worksheets are loaded on-demand:

```csharp
using var workbook = new Workbook("large-file.xlsx");

// Only loads Sheet1 when first accessed
var sheet1 = workbook.GetWorksheet("Sheet1");

// Subsequent accesses are cached
var sheet1Again = workbook.GetWorksheet("Sheet1"); // Fast
```

### Memory Usage

- **Small files** (< 10MB): Entire sheet loaded in memory
- **Large files** (> 100MB): Streaming approach recommended
- **Optimization**: Process rows in batches, dispose workbook after use

### Row Count

The `RowCount` property reflects data rows (excluding headers):

```csharp
var sheet = workbook.GetWorksheet("Data");
Console.WriteLine($"Data rows: {sheet.RowCount}");
// For 100 rows total: RowCount = 99 (excluding header)
```

## Best Practices

### 1. Always Dispose Workbook

```csharp
// ✅ Good: Using statement
using var workbook = new Workbook("data.xlsx");
{
    // Use workbook
}
// Automatically disposed

// ✅ Good: Manual disposal
var workbook = new Workbook("data.xlsx");
try
{
    // Use workbook
}
finally
{
    workbook.Dispose();
}

// ❌ Bad: No disposal
var workbook = new Workbook("data.xlsx");
var data = workbook.GetWorksheet("Data").GetAllRows(); // File handle leaked
```

### 2. Validate Column Existence

```csharp
var sheet = workbook.GetWorksheet("Data");

// ✅ Good: Check headers
if (!sheet.Headers.Contains("Email"))
{
    throw new InvalidOperationException("Email column not found");
}

// ❌ Bad: Assume column exists
var email = row["Email"]; // May throw KeyNotFoundException
```

### 3. Handle Type Conversions

```csharp
var row = sheet.GetRow(1);

// ✅ Good: Safe conversion
var salary = row.ContainsKey("Salary")
    ? Convert.ToDouble(row["Salary"])
    : 0.0;

// ❌ Bad: Direct cast
var salary = (double)row["Salary"]; // May throw InvalidCastException
```

### 4. Batch Processing for Large Files

```csharp
// ✅ Good: Process in batches
const int batchSize = 100;
var batch = new List<Dictionary<string, object>>();

foreach (var row in sheet.GetAllRows())
{
    batch.Add(row);
    
    if (batch.Count >= batchSize)
    {
        ProcessBatch(batch);
        batch.Clear();
    }
}

if (batch.Count > 0)
    ProcessBatch(batch);
```

## Limitations

- ❌ CSV files: Use separate CSV parsing library
- ❌ Formulas: Returns calculated values, not formula text
- ❌ Merged cells: Treated as individual cells
- ✅ Large files: Supports streaming access
- ✅ Multiple sheets: Access by name or index
- ✅ Mixed data types: Automatic type detection

## Thread Safety

- ❌ `Workbook` and `Worksheet` are **NOT thread-safe**
- ✅ Multiple `Workbook` instances can be used in parallel
- ✅ Create separate `Workbook` instance per thread

---

Next: [Slide Module](slide-module.md) | [Overview](overview.md)

