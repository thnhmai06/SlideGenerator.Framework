using ClosedXML.Excel;
using SlideGenerator.Framework.Sheet.Models;

namespace SlideGenerator.Framework.Tests.Sheet;

[TestClass]
public class WorkbookTests
{
    [TestMethod]
    public void Workbook_Constructor_LoadsWorksheetsCorrectly()
    {
        // Arrange
        using var xlWorkbook = new XLWorkbook();
        var sheet1 = xlWorkbook.Worksheets.Add("Sheet1");
        sheet1.Cell(1, 1).Value = "Header";
        sheet1.Cell(2, 1).Value = "Data";

        var sheet2 = xlWorkbook.Worksheets.Add("Sheet2");
        sheet2.Cell(1, 1).Value = "Test";

        // Act
        using var workbook = new Workbook(xlWorkbook);

        // Assert
        Assert.HasCount(2, workbook.Worksheets);
        Assert.IsTrue(workbook.Worksheets.ContainsKey("Sheet1"));
        Assert.IsTrue(workbook.Worksheets.ContainsKey("Sheet2"));

        var ws1 = workbook.Worksheets["Sheet1"];
        Assert.AreEqual(1, ws1.RowCount); // Header (1) + Data (2) -> Max(2) - Min(1) = 1 row of data
    }

    [TestMethod]
    public void GetWorksheetsInfo_ReturnsCorrectRowCounts()
    {
        // Arrange
        using var xlWorkbook = new XLWorkbook();
        var sheet = xlWorkbook.Worksheets.Add("DataSheet");
        sheet.Cell(1, 1).Value = "H1";
        sheet.Cell(2, 1).Value = "V1";
        sheet.Cell(3, 1).Value = "V2";

        using var workbook = new Workbook(xlWorkbook);

        // Act
        var info = workbook.GetWorksheetsInfo();

        // Assert
        Assert.HasCount(1, info);
        Assert.IsTrue(info.ContainsKey("DataSheet"));
        Assert.AreEqual(2, info["DataSheet"]); // Header (1) + V1(2) + V2(3) -> Max(3) - Min(1) = 2
    }
}