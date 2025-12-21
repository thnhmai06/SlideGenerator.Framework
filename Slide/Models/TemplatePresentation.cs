using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SlideGenerator.Framework.Slide.DTOs;
using SlideGenerator.Framework.Slide.Exceptions;
using Spire.Presentation;
using Spire.Presentation.Drawing;

namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Represents a template presentation with a single slide that can be used to generate multiple slides.
/// </summary>
public sealed class TemplatePresentation : Presentation
{
    private readonly Spire.Presentation.Presentation _spirePresentation = new();
    private readonly ISlide _spireMainSlide;
    private readonly string? _tempCopyPath;

    /// <summary>
    ///     Opens a template presentation.
    /// </summary>
    /// <param name="filePath">Path to the presentation file.</param>
    /// <param name="slideIndex">Slide index to use as template (1-based).</param>
    public TemplatePresentation(string filePath, int slideIndex = 1) : base(filePath, false)
    {
        MainSlideIndex = slideIndex;
        slideIndex--; // Convert to zero-based index

        var slideIds = GetSlideIdList()?.ChildElements;
        var slideId = (SlideId?)slideIds?[slideIndex];
        MainSlideRelationshipId = slideId?.RelationshipId?.Value
                                  ?? throw new InvalidPresentation(
                                      filePath, $"Slide index {MainSlideIndex} does not exist.");

        try
        {
            _spirePresentation.LoadFromFile(filePath);
        }
        catch (IOException)
        {
            var tempCopyPath = TryCreateTempCopy(filePath);
            if (string.IsNullOrWhiteSpace(tempCopyPath))
                throw;

            _spirePresentation.LoadFromFile(tempCopyPath);
            _tempCopyPath = tempCopyPath;
        }
        _spireMainSlide = _spirePresentation.Slides[slideIndex];
    }

    /// <summary>
    ///     The Index of the main slide used as a template (1-based).
    /// </summary>
    public int MainSlideIndex { get; }

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

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        _spirePresentation.Dispose();
        TryDeleteTempCopy(_tempCopyPath);
    }

    private static string? TryCreateTempCopy(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"sg-template-{Guid.NewGuid():N}{extension}");
            File.Copy(filePath, tempPath, true);
            return tempPath;
        }
        catch
        {
            return null;
        }
    }

    private static void TryDeleteTempCopy(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
