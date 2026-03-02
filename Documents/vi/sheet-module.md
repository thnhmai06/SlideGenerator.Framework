# Tài liệu Sheet Module

[🇬🇧 English Version](../en/sheet-module.md)

## Tổng Quan

Sheet module cung cấp API nhẹ nhàng nhưng mạnh mẽ để đọc dữ liệu từ các tệp Excel (.xlsx) và nguồn CSV. Nó trừu tượng hóa độ phức tạp của OpenXML trong khi vẫn giữ tính linh hoạt cho các mẫu trích xuất dữ liệu khác nhau.

## Kiến Trúc

### Các Thành Phần Chính

#### Workbook (Sổ Làm Việc)

Đại diện cho một tệp Excel (.xlsx):

```csharp
namespace SlideGenerator.Framework.Features.Sheet.Models;

public sealed class Workbook : IDisposable
{
    /// <summary>
    ///     Lấy tập hợp các trang tính trong sổ làm việc này.
    /// </summary>
    public ICollection<Worksheet> Worksheets { get; }
    
    /// <summary>
    ///     Lấy hoặc tạo một trang tính theo tên.
    /// </summary>
    /// <param name="name">Tên trang tính (không phân biệt chữ hoa/thường).</param>
    /// <returns>Trang tính, hoặc null nếu không tìm thấy.</returns>
    public Worksheet? GetWorksheet(string name);
    
    /// <summary>
    ///     Lấy hoặc tạo một trang tính theo chỉ số (0-based).
    /// </summary>
    /// <param name="index">Chỉ số trang tính.</param>
    /// <returns>Trang tính, hoặc null nếu chỉ số ngoài phạm vi.</returns>
    public Worksheet? GetWorksheet(int index);
    
    /// <summary>
    ///     Giải phóng sổ làm việc và phát hành các tay cầm tệp.
    /// </summary>
    public void Dispose();
}
```

#### Worksheet (Trang Tính)

Đại diện cho một trang tính trong sổ làm việc:

```csharp
public sealed class Worksheet
{
    /// <summary>
    ///     Lấy tên của trang tính này.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///     Lấy số lượng hàng có dữ liệu.
    /// </summary>
    public int RowCount { get; }
    
    /// <summary>
    ///     Lấy hàng tiêu đề (hàng đầu tiên có dữ liệu).
    /// </summary>
    public IReadOnlyList<string> Headers { get; }
    
    /// <summary>
    ///     Lấy một hàng cụ thể dưới dạng từ điển tên cột → giá trị.
    /// </summary>
    /// <param name="rowIndex">Chỉ số hàng 0-based (0 = tiêu đề, 1+ = hàng dữ liệu).</param>
    /// <returns>Từ điển có tên cột làm khóa, giá trị ô làm giá trị.</returns>
    public Dictionary<string, object> GetRow(int rowIndex);
    
    /// <summary>
    ///     Lấy tất cả các hàng dữ liệu (không bao gồm tiêu đề).
    /// </summary>
    /// <returns>Tập hợp các từ điển hàng.</returns>
    public IEnumerable<Dictionary<string, object>> GetAllRows();
    
    /// <summary>
    ///     Lấy giá trị ô cụ thể.
    /// </summary>
    /// <param name="row">Chỉ số hàng 0-based.</param>
    /// <param name="column">Chỉ số cột 0-based.</param>
    /// <returns>Giá trị ô dưới dạng object (chuỗi, số, ngày, v.v.)</returns>
    public object? GetCell(int row, int column);
}
```

## Cách Sử Dụng

### Tải Sổ Làm Việc

```csharp
using SlideGenerator.Framework.Features.Sheet.Models;

// Tải từ tệp
using var workbook = new Workbook("data.xlsx");

// Truy cập trang tính theo tên
var sheet = workbook.GetWorksheet("Nhân Viên");
if (sheet == null)
    throw new InvalidOperationException("Không tìm thấy trang tính 'Nhân Viên'");
```

### Đọc Dữ Liệu

#### Truy Cập Từng Hàng

```csharp
using var workbook = new Workbook("data.xlsx");
var sheet = workbook.GetWorksheet("Nhân Viên");

// Lấy tiêu đề
Console.WriteLine($"Cột: {string.Join(", ", sheet.Headers)}");
// Đầu ra: Cột: Tên, Chức Vụ, Phòng Ban, Email

// Đọc hàng cụ thể
var row1 = sheet.GetRow(1); // Hàng dữ liệu đầu tiên
Console.WriteLine($"Tên: {row1["Tên"]}");
Console.WriteLine($"Chức Vụ: {row1["Chức Vụ"]}");
```

#### Tất Cả Hàng Cùng Lúc

```csharp
var sheet = workbook.GetWorksheet("Nhân Viên");

foreach (var row in sheet.GetAllRows())
{
    var tên = row["Tên"];
    var chứcVụ = row["Chức Vụ"];
    var phòngBan = row["Phòng Ban"];
    
    Console.WriteLine($"{tên} - {chứcVụ} ({phòngBan})");
}
```

#### Truy Cập Ô Trực Tiếp

```csharp
var sheet = workbook.GetWorksheet("Nhân Viên");

// Lấy ô theo vị trí (0-based)
var cell = sheet.GetCell(1, 0); // Hàng 1, Cột 0
Console.WriteLine($"Giá Trị: {cell}");
```

### Xử Lý Kiểu Dữ Liệu

Các ô Excel được trả về dưới dạng object với các kiểu phù hợp:

```csharp
var row = sheet.GetRow(1);

// Giá trị chuỗi
var tên = (string)row["Tên"];

// Giá trị số
var lương = Convert.ToDouble(row["Lương"]);

// Giá trị ngày
var ngàyVàoLàm = (DateTime)row["Ngày Vào Làm"];

// Ô trống/null
if (row["TrườngTùyChọn"] == null)
    Console.WriteLine("Ô trống");
```

### Xử Lý Lỗi

```csharp
try
{
    using var workbook = new Workbook("data.xlsx");
    var sheet = workbook.GetWorksheet("Không Tồn Tại");
    
    if (sheet == null)
        throw new InvalidOperationException("Không tìm thấy trang tính");
    
    var row = sheet.GetRow(999); // Có thể ngoài phạm vi
}
catch (FileNotFoundException)
{
    // Tệp không tồn tại
}
catch (InvalidOperationException)
{
    // Định dạng Excel không hợp lệ
}
```

## Mẫu Thường Gặp

### Tạo Trang Trình Bày Dựa Trên Mẫu

```csharp
public class SlideGenerator
{
    public async Task GenerateFromExcel(string templatePath, string dataPath)
    {
        using var workbook = new Workbook(dataPath);
        var sheet = workbook.GetWorksheet("Dữ Liệu");
        
        foreach (var row in sheet.GetAllRows())
        {
            // Sử dụng dữ liệu hàng để sao chép và điền vào trang trình bày
            await GenerateSlide(templatePath, row);
        }
    }
}
```

### Xác Thực Dữ Liệu

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
                Console.WriteLine($"Thiếu cột bắt buộc: {column}");
                return false;
            }
        }
        
        return true;
    }
}
```

## Xem Xét Hiệu Suất

### Tải Lười Biếng

Các trang tính được tải theo yêu cầu:

```csharp
using var workbook = new Workbook("large-file.xlsx");

// Chỉ tải Trang1 khi lần đầu truy cập
var sheet1 = workbook.GetWorksheet("Trang1");

// Các lần truy cập tiếp theo nhanh
var sheet1Again = workbook.GetWorksheet("Trang1");
```

### Sử Dụng Bộ Nhớ

- **Tệp nhỏ** (< 10MB): Toàn bộ trang tính được tải vào bộ nhớ
- **Tệp lớn** (> 100MB): Nên sử dụng cách tiếp cận streaming
- **Tối ưu hóa**: Xử lý hàng theo từng batch, giải phóng workbook sau khi sử dụng

## Thực Hành Tốt Nhất

### 1. Luôn Giải Phóng Workbook

```csharp
// ✅ Tốt: Sử dụng using
using var workbook = new Workbook("data.xlsx");
{
    // Sử dụng workbook
}
// Tự động giải phóng

// ❌ Xấu: Không giải phóng
var workbook = new Workbook("data.xlsx");
var data = workbook.GetWorksheet("Dữ Liệu").GetAllRows();
// Tay cầm tệp bị rò rỉ
```

### 2. Xác Thực Tồn Tại Cột

```csharp
var sheet = workbook.GetWorksheet("Dữ Liệu");

// ✅ Tốt: Kiểm tra tiêu đề
if (!sheet.Headers.Contains("Email"))
{
    throw new InvalidOperationException("Không tìm thấy cột Email");
}

// ❌ Xấu: Giả định cột tồn tại
var email = row["Email"]; // Có thể ném KeyNotFoundException
```

### 3. Chuyển Đổi Kiểu An Toàn

```csharp
var row = sheet.GetRow(1);

// ✅ Tốt: Chuyển đổi an toàn
var lương = row.ContainsKey("Lương")
    ? Convert.ToDouble(row["Lương"])
    : 0.0;

// ❌ Xấu: Ép kiểu trực tiếp
var lương = (double)row["Lương"]; // Có thể ném InvalidCastException
```

## Hạn Chế

- ❌ Tệp CSV: Sử dụng thư viện phân tích CSV riêng
- ❌ Công thức: Trả về giá trị được tính toán, không phải văn bản công thức
- ❌ Ô được gộp: Được coi là ô riêng lẻ
- ✅ Tệp lớn: Hỗ trợ truy cập streaming
- ✅ Nhiều trang tính: Truy cập theo tên hoặc chỉ số
- ✅ Kiểu dữ liệu hỗn hợp: Phát hiện kiểu tự động

## An Toàn Luồng

- ❌ `Workbook` và `Worksheet` **KHÔNG an toàn luồng**
- ✅ Nhiều instance `Workbook` có thể được sử dụng song song
- ✅ Tạo instance `Workbook` riêng cho mỗi luồng

---

Tiếp theo: [Slide Module](slide-module.md) | [Tổng Quan](overview.md)

