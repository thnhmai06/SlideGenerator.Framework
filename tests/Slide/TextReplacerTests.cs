using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Framework.Slide;
// For TextBody, Paragraph, Run, Text
using Shape = DocumentFormat.OpenXml.Presentation.Shape; // Explicit alias
using NonVisualShapeProperties = DocumentFormat.OpenXml.Presentation.NonVisualShapeProperties;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties;
using NonVisualShapeDrawingProperties = DocumentFormat.OpenXml.Presentation.NonVisualShapeDrawingProperties;
using ApplicationNonVisualDrawingProperties = DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties;
using ShapeProperties = DocumentFormat.OpenXml.Presentation.ShapeProperties;
using TextBody = DocumentFormat.OpenXml.Presentation.TextBody;
using Text = DocumentFormat.OpenXml.Drawing.Text; // Text inside Run is Drawing.Text

namespace SlideGenerator.Framework.Tests.Slide;

[TestClass]
public class TextReplacerTests
{
    [TestMethod]
    public void ScanPlaceholders_String_ReturnsCorrectPlaceholders()
    {
        var text = "Hello {{Name}}, welcome to {{Place}}!";
        var result = TextReplacer.ScanPlaceholders(text);

        Assert.AreEqual(2, result.Count);
        CollectionAssert.Contains(result.ToList(), "Name");
        CollectionAssert.Contains(result.ToList(), "Place");
    }

    [TestMethod]
    public async Task ReplaceAsync_InMemoryPresentation_ReplacesText()
    {
        // Setup
        using var stream = new MemoryStream();
        using (var doc = PresentationDocument.Create(stream, PresentationDocumentType.Presentation))
        {
            var presentationPart = doc.AddPresentationPart();
            presentationPart.Presentation = new Presentation();
            var slidePart = presentationPart.AddNewPart<SlidePart>();

            // Construct a minimal valid slide structure with a Shape containing TextBody
            var slide = new DocumentFormat.OpenXml.Presentation.Slide(
                new CommonSlideData(
                    new ShapeTree(
                        new Shape(
                            new NonVisualShapeProperties(
                                new NonVisualDrawingProperties { Id = 1, Name = "Shape 1" },
                                new NonVisualShapeDrawingProperties(),
                                new ApplicationNonVisualDrawingProperties()
                            ),
                            new ShapeProperties(),
                            new TextBody(
                                new Paragraph(
                                    new Run(
                                        new Text("Hello {{Name}}")
                                    )
                                )
                            )
                        )
                    )
                )
            );
            slidePart.Slide = slide;
            slidePart.Slide.Save();

            var replacements = new Dictionary<string, string> { { "Name", "World" } };

            // Act
            var count = await TextReplacer.ReplaceAsync(slidePart, replacements);

            // Assert
            Assert.AreEqual((uint)1, count);
            var innerText = slidePart.Slide.InnerText;
            StringAssert.Contains(innerText, "Hello World");
        }
    }
}