using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Framework.Slide.Exceptions;
using Spire.Presentation;
using Spire.Presentation.Drawing;

namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Represents a template presentation with a single slide that can be used to generate multiple slides.
/// </summary>
public sealed class TemplatePresentation : Presentation
{
    private readonly ISlide _spireMainSlide;
    private readonly Spire.Presentation.Presentation _spirePresentation;

    /// <summary>
    ///     Opens a template presentation.
    /// </summary>
    /// <param name="filePath">Path to the presentation file.</param>
    public TemplatePresentation(string filePath) : base(filePath, false)
    {
        if (SlideCount != 1)
            throw new NotOnlyOneSlidePresentation(filePath);

        var slideIndex = 0; // First slide, convert to zero-based index

        var slideIds = GetSlideIdList()?.ChildElements;
        var slideId = (SlideId?)slideIds?[slideIndex];
        MainSlideRelationshipId = slideId?.RelationshipId?.Value
                                  ?? throw new InvalidPresentation(
                                      filePath, $"Slide index {slideIndex + 1} does not exist.");

        // Spire
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _spirePresentation = new Spire.Presentation.Presentation();
        _spirePresentation.LoadFromStream(fs, FileFormat.Auto);
        _spireMainSlide = _spirePresentation.Slides[slideIndex];
    }

    /// <summary>
    ///     Gets the relationship ID of the main slide.
    /// </summary>
    public string MainSlideRelationshipId { get; }

    /// <summary>
    ///     Gets the main slide part.
    /// </summary>
    public SlidePart? GetSlidePart()
    {
        return GetSlidePart(MainSlideRelationshipId);
    }

    /// <summary>
    ///     Saves the template presentation as a working presentation.
    /// </summary>
    /// <param name="destPath">Destination path for the new working presentation.</param>
    /// <returns>A new instance of WorkingPresentation.</returns>
    public WorkingPresentation SaveAs(string destPath)
    {
        var newDoc = Doc.Clone(destPath, true);
        newDoc.PresentationPart!.Presentation.Save();
        return new WorkingPresentation(newDoc, destPath);
    }

    /// <summary>
    ///     Gets all preview of image shapes from the template slide.
    /// </summary>
    /// <returns>A dictionary mapping shape IDs to shape image info.</returns>
    public Dictionary<uint, ImageShapePreview> GetAllPreviewImageShapes()
    {
        Dictionary<uint, ImageShapePreview> shapes = [];
        foreach (var shape in _spireMainSlide.Shapes.ToArray())
        {
            if (shape.IsHidden) continue;
            if (shape is SlidePicture || shape.Fill.FillType == FillFormatType.Picture)
            {
                using var imageStream = shape.SaveAsImage();
                using var ms = new MemoryStream();
                imageStream.CopyTo(ms);
                shapes.Add(shape.Id, new ImageShapePreview(shape.Name, ms.ToArray()));
            }
        }

        return shapes;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        _spirePresentation.Dispose();
    }
}