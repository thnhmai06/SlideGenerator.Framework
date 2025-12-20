using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Path = System.IO.Path;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using PresentationText = DocumentFormat.OpenXml.Presentation.Text;
using DrawingText = DocumentFormat.OpenXml.Drawing.Text;

namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Base class for presentation manipulation.
/// </summary>
public abstract class Presentation : IDisposable
{
    internal readonly PresentationDocument Doc;
    private bool _disposed;

    /// <summary>
    ///     Creates a new presentation instance.
    /// </summary>
    /// <param name="filePath">Path to the presentation file.</param>
    /// <param name="isEditable">Whether the presentation should be opened for editing.</param>
    protected Presentation(string filePath, bool isEditable)
    {
        FilePath = filePath;
        Doc = !Path.Exists(filePath)
            ? PresentationDocument.Create(filePath, PresentationDocumentType.Presentation)
            : PresentationDocument.Open(filePath, isEditable);
    }

    /// <summary>
    ///     Creates a new presentation instance from existing OpenXML presentation document.
    /// </summary>
    /// <param name="doc">The OpenXML presentation document.</param>
    /// <param name="filePath">Path to the presentation file.</param>
    internal Presentation(PresentationDocument doc, string filePath)
    {
        FilePath = filePath;
        Doc = doc;
    }

    /// <summary>
    ///     Gets the file path of the presentation.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     Gets the number of slides in the presentation.
    /// </summary>
    public int SlideCount => GetSlideIdList()?.Count() ?? 0;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Doc.Dispose();
        Dispose(_disposed);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {}

    /// <summary>
    ///     Gets the presentation part.
    /// </summary>
    protected PresentationPart? GetPresentationPart()
    {
        return Doc.PresentationPart;
    }

    /// <summary>
    ///     Gets the slide ID list.
    /// </summary>
    public SlideIdList? GetSlideIdList()
    {
        return GetPresentationPart()?.Presentation.SlideIdList;
    }

    /// <summary>
    ///     Gets a slide part by relationship ID.
    /// </summary>
    /// <param name="slideRId">The relationship ID of the slide.</param>
    public SlidePart? GetSlidePart(string slideRId)
    {
        return (SlidePart?)GetPresentationPart()?.GetPartById(slideRId);
    }

    /// <summary>
    ///     Gets all presentation text elements from a slide.
    /// </summary>
    public static IEnumerable<PresentationText> GetPresentationTexts(SlidePart slidePart)
    {
        return slidePart.Slide.Descendants<PresentationText>();
    }


    /// <summary>
    ///     Gets all drawing text elements from a slide.
    /// </summary>
    public static IEnumerable<DrawingText> GetDrawingTexts(SlidePart slidePart)
    {
        List<DrawingText> texts = [];
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
    {
        return slidePart.Slide.Descendants<Shape>();
    }

    /// <summary>
    ///     Gets a shape by its ID.
    /// </summary>
    public static Shape? GetShapeById(SlidePart slidePart, uint shapeId)
    {
        return GetShapes(slidePart).FirstOrDefault(shape
            => shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
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
        return GetPictures(slidePart).FirstOrDefault(pic
            => pic.NonVisualPictureProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
    }
}