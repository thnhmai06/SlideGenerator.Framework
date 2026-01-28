using ClosedXML.Excel;
using SlideGenerator.Framework.Sheet;

namespace SlideGenerator.Framework.Tests.Sheet;

[TestClass]
public class WorkbookTests
{
    [TestMethod]
    public void GetSheetsRowCount_ReturnsCorrectRowCounts()
    {
        // Arrange
        using var xlWorkbook = new XLWorkbook();
        var sheet1 = xlWorkbook.Worksheets.Add("Sheet1");
        sheet1.Cell(1, 1).Value = "Header";
        sheet1.Cell(2, 1).Value = "Data";

        var sheet2 = xlWorkbook.Worksheets.Add("Sheet2");
        sheet2.Cell(1, 1).Value = "Test";

        // Act
        var result = WorkbookService.GetSheetsRowCount(xlWorkbook);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.ContainsKey("Sheet1"));
        Assert.IsTrue(result.ContainsKey("Sheet2"));
        
        Assert.AreEqual(1, result["Sheet1"]); // Header (1) + Data (2) -> Max(2) - Min(1) = 1 row of data
        Assert.AreEqual(0, result["Sheet2"]); // Only Header
    }
}
