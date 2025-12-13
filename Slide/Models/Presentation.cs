using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Core.Slide.Exceptions;
using BlipFill = DocumentFormat.OpenXml.Presentation.BlipFill;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using Text = DocumentFormat.OpenXml.Presentation.Text;

namespace SlideGenerator.Core.Slide.Models;

/// <summary>
///     Base class for presentation manipulation.
/// </summary>
public abstract class Presentation : IDisposable
{
    private readonly PresentationDocument _doc;
    private bool _disposed;

    /// <summary>
    ///     Creates a new presentation instance.
    /// </summary>
    /// <param name="filePath">Path to the presentation file.</param>
    /// <param name="isEditable">Whether the presentation should be opened for editing.</param>
    protected Presentation(string filePath, bool isEditable)
    {
        FilePath = filePath;
        _doc = PresentationDocument.Open(filePath, isEditable);
    }

    /// <summary>
    ///     Gets the file path of the presentation.
    /// </summary>
    public string FilePath { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _doc.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Gets the presentation part.
    /// </summary>
    protected PresentationPart GetPresentationPart()
    {
        return _doc.PresentationPart
               ?? throw new InvalidPresentationException(FilePath, "Missing presentation part.");
    }

    /// <summary>
    ///     Gets the slide ID list.
    /// </summary>
    public SlideIdList GetSlideIdList()
    {
        return GetPresentationPart().Presentation.SlideIdList
               ?? throw new InvalidPresentationException(FilePath, "Missing slide ID list.");
    }

    /// <summary>
    ///     Gets a slide part by relationship ID.
    /// </summary>
    /// <param name="slideRId">The relationship ID of the slide.</param>
    public SlidePart GetSlidePart(string slideRId)
    {
        return (SlidePart)GetPresentationPart().GetPartById(slideRId);
    }

    /// <summary>
    ///     Gets all presentation text elements from a slide.
    /// </summary>
    public static IEnumerable<Text> GetPresentationTexts(SlidePart slidePart)
    {
        return slidePart.Slide.Descendants<Text>();
    }

    /// <summary>
    ///     Gets all drawing text elements from a slide.
    /// </summary>
    public static IEnumerable<DocumentFormat.OpenXml.Drawing.Text> GetDrawingTexts(SlidePart slidePart)
    {
        List<DocumentFormat.OpenXml.Drawing.Text> texts = [];
        var shapes = GetShapes(slidePart);
        foreach (var shape in shapes)
        {
            if (shape.TextBody is null) continue;

            foreach (var paragraph in shape.TextBody.Descendants<Paragraph>())
            foreach (var run in paragraph.Descendants<Run>())
                if (run.Text is not null)
                    texts.Add(run.Text);
        }

        return texts;
    }

    /// <summary>
    ///     Gets all shapes from a slide.
    /// </summary>
    /// <param name="slidePart">The slide part.</param>
    /// <param name="imageShapesOnly">If true, only returns shapes filled with images.</param>
    public static IEnumerable<Shape> GetShapes(SlidePart slidePart, bool imageShapesOnly = false)
    {
        var shapes = slidePart.Slide.Descendants<Shape>();
        if (imageShapesOnly)
            return shapes.Where(shape =>
            {
                var fill = shape.ShapeProperties?.GetFirstChild<FillProperties>();
                return fill?.GetFirstChild<BlipFill>() != null;
            });

        return shapes;
    }

    /// <summary>
    ///     Gets a shape by its ID.
    /// </summary>
    public static Shape? GetShapeById(SlidePart slidePart, uint shapeId)
    {
        return GetShapes(slidePart)
            .FirstOrDefault(shape => shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
    }

    /// <summary>
    ///     Gets all pictures from a slide.
    /// </summary>
    public static IEnumerable<Picture> GetPictures(SlidePart slidePart)
    {
        return slidePart.Slide.Descendants<Picture>();
    }

    /// <summary>
    ///     Gets a picture by its ID.
    /// </summary>
    public static Picture? GetPictureById(SlidePart slidePart, uint shapeId)
    {
        return GetPictures(slidePart)
            .FirstOrDefault(pic => pic.NonVisualPictureProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
    }
}