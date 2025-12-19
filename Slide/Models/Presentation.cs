using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Framework.Slide.Exceptions;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using Text = DocumentFormat.OpenXml.Presentation.Text;

namespace SlideGenerator.Framework.Slide.Models;

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

    /// <summary>
    ///     Gets the number of slides in the presentation.
    /// </summary>
    public int SlideCount => GetSlideIdList().Count();

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
    /// <exception cref="InvalidPresentation">Thrown if the presentation part is missing.</exception>
    protected PresentationPart GetPresentationPart()
        => _doc.PresentationPart
               ?? throw new InvalidPresentation(FilePath, "Missing presentation part.");

    /// <summary>
    ///     Gets the slide ID list.
    /// </summary>
    /// <exception cref="InvalidPresentation">Thrown if the slide ID list is missing.</exception>
    public SlideIdList GetSlideIdList()
        => GetPresentationPart().Presentation.SlideIdList
               ?? throw new InvalidPresentation(FilePath, "Missing slide ID list.");

    /// <summary>
    ///     Gets a slide part by relationship ID.
    /// </summary>
    /// <param name="slideRId">The relationship ID of the slide.</param>
    public SlidePart GetSlidePart(string slideRId)
        => (SlidePart)GetPresentationPart().GetPartById(slideRId);

    /// <summary>
    ///     Gets all presentation text elements from a slide.
    /// </summary>
    public static IEnumerable<Text> GetPresentationTexts(SlidePart slidePart)
        => slidePart.Slide.Descendants<Text>();


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
    public static IEnumerable<Shape> GetShapes(SlidePart slidePart)
        => slidePart.Slide.Descendants<Shape>();

    /// <summary>
    ///     Gets a shape by its ID.
    /// </summary>
    public static Shape? GetShapeById(SlidePart slidePart, uint shapeId)
        => GetShapes(slidePart).FirstOrDefault(shape
            => shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);

    /// <summary>
    ///     Gets all pictures from a slide.
    /// </summary>
    public static IEnumerable<Picture> GetPictures(SlidePart slidePart)
        => slidePart.Slide.Descendants<Picture>();

    /// <summary>
    ///     Gets a picture by its ID.
    /// </summary>
    public static Picture? GetPictureById(SlidePart slidePart, uint shapeId)
        => GetPictures(slidePart).FirstOrDefault(pic
            => pic.NonVisualPictureProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
}