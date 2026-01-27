using DocumentFormat.OpenXml.Drawing;
using SlideGenerator.Framework.Slide;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using Picture = DocumentFormat.OpenXml.Presentation.Picture;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties;
using NonVisualPictureProperties = DocumentFormat.OpenXml.Presentation.NonVisualPictureProperties;
using NonVisualShapeProperties = DocumentFormat.OpenXml.Presentation.NonVisualShapeProperties;
using BlipFill = DocumentFormat.OpenXml.Presentation.BlipFill;
using Transform2D = DocumentFormat.OpenXml.Drawing.Transform2D;
using Extents = DocumentFormat.OpenXml.Drawing.Extents;
using ShapeProperties = DocumentFormat.OpenXml.Presentation.ShapeProperties;
using NonVisualPictureDrawingProperties = DocumentFormat.OpenXml.Presentation.NonVisualPictureDrawingProperties;
using ApplicationNonVisualDrawingProperties = DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties;
using NonVisualShapeDrawingProperties = DocumentFormat.OpenXml.Presentation.NonVisualShapeDrawingProperties;
using TextBody = DocumentFormat.OpenXml.Presentation.TextBody;

namespace SlideGenerator.Framework.Tests.Slide;

[TestClass]
public class ImageReplacerTests
{
    [TestMethod]
    public void GetPictureSize_ReturnsCorrectSize()
    {
        // Arrange
        var widthEmu = 952500L; // 100 pixels * 9525
        var heightEmu = 476250L; // 50 pixels * 9525

        var picture = new Picture(
            new NonVisualPictureProperties(
                new NonVisualDrawingProperties { Id = 1, Name = "Pic 1" },
                new NonVisualPictureDrawingProperties(),
                new ApplicationNonVisualDrawingProperties()),
            new BlipFill(),
            new ShapeProperties(
                new Transform2D(
                    new Offset { X = 0, Y = 0 },
                    new Extents { Cx = widthEmu, Cy = heightEmu }
                )
            )
        );

        // Act
        var size = SlideService.GetPictureSize(picture);

        // Assert
        Assert.AreEqual(100, size.Width);
        Assert.AreEqual(50, size.Height);
    }

    [TestMethod]
    public void GetShapeSize_ReturnsCorrectSize()
    {
        // Arrange
        var widthEmu = 1905000L; // 200 pixels
        var heightEmu = 952500L; // 100 pixels

        var shape = new Shape(
            new NonVisualShapeProperties(
                new NonVisualDrawingProperties { Id = 2, Name = "Shape 1" },
                new NonVisualShapeDrawingProperties(),
                new ApplicationNonVisualDrawingProperties()),
            new ShapeProperties(
                new Transform2D(
                    new Offset { X = 0, Y = 0 },
                    new Extents { Cx = widthEmu, Cy = heightEmu }
                )
            ),
            new TextBody()
        );

        // Act
        var size = SlideService.GetShapeSize(shape);

        // Assert
        Assert.AreEqual(200, size.Width);
        Assert.AreEqual(100, size.Height);
    }
}