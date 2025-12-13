using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Core.Slide.DTOs;
using SlideGenerator.Core.Slide.Exceptions;
using Spire.Presentation;
using Spire.Presentation.Drawing;

namespace SlideGenerator.Core.Slide.Models;

/// <summary>
///     Represents a template presentation with a single slide that can be used to generate multiple slides.
/// </summary>
public sealed class TemplatePresentation : Presentation
{
    private const int FirstSlideIndex = 0;
    private readonly ISlide _spireMainSlide;
    private readonly Spire.Presentation.Presentation _spirePresentation = new();

    /// <summary>
    ///     Opens a template presentation.
    /// </summary>
    /// <param name="filePath">Path to the presentation file.</param>
    /// <exception cref="InvalidTemplateException">Thrown when the presentation does not have exactly one slide.</exception>
    public TemplatePresentation(string filePath) : base(filePath, true)
    {
        var slideIds = GetSlideIdList().ChildElements;
        if (slideIds.Count != 1)
            throw new InvalidTemplateException(filePath, slideIds.Count);

        var slideId = (SlideId)slideIds[FirstSlideIndex];
        MainSlideRelationshipId = slideId.RelationshipId?.Value
                                  ?? throw new InvalidPresentationException(filePath,
                                      $"No relationship ID for slide {FirstSlideIndex + 1}.");

        _spirePresentation.LoadFromFile(filePath);
        _spireMainSlide = _spirePresentation.Slides[FirstSlideIndex];
    }

    /// <summary>
    ///     Gets the relationship ID of the main slide.
    /// </summary>
    public string MainSlideRelationshipId { get; }

    /// <summary>
    ///     Gets the main slide part.
    /// </summary>
    public SlidePart GetMainSlidePart()
    {
        return GetSlidePart(MainSlideRelationshipId);
    }

    /// <summary>
    ///     Gets all preview of image shapes from the template slide.
    /// </summary>
    /// <returns>A dictionary mapping shape IDs to shape image info.</returns>
    public Dictionary<uint, ShapeImagePreview> GetAllPreviewImageShapes()
    {
        Dictionary<uint, ShapeImagePreview> shapes = [];
        foreach (var shape in _spireMainSlide.Shapes.ToArray())
        {
            if (shape.IsHidden) continue;
            if (shape is SlidePicture || shape.Fill.FillType == FillFormatType.Picture)
            {
                using var imageStream = shape.SaveAsImage();
                using var ms = new MemoryStream();
                imageStream.CopyTo(ms);
                shapes.Add(shape.Id, new ShapeImagePreview(shape.Name, ms.ToArray()));
            }
        }

        return shapes;
    }
}