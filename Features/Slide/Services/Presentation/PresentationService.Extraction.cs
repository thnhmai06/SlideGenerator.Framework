using SlideGenerator.Framework.Slide.Models;
using Spire.Presentation;
using Spire.Presentation.Drawing;

namespace SlideGenerator.Framework.Slide;

public static partial class PresentationService
{
    /// <summary>
    ///     Gets all preview of picture or image-filled shapes from the presentation file.
    /// </summary>
    /// <remarks>
    ///     This method uses <see cref="Presentation" /> to render shapes to images, which requires loading the file from disk.
    /// </remarks>
    /// <param name="filePath">Path to the presentation file.</param>
    /// <param name="index">
    ///     The 1-based index of the slide to extract shapes from. Index can be only in [1, 10] due to
    ///     FreeSpire.Presentation limitations.
    /// </param>
    /// <returns>A dictionary mapping shape IDs to shape image info.</returns>
    public static Dictionary<uint, ImageShapePreview> ExtractAllImageShapes(string filePath, int index = 1)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var spirePresentation = new Presentation();
        spirePresentation.LoadFromStream(fs, FileFormat.Auto);

        return ExtractAllImageShapes(spirePresentation, index);
    }

    /// <summary>
    ///     Gets all preview of picture or image-filled shapes from the presentation file.
    /// </summary>
    /// <remarks>
    ///     This method uses <see cref="Presentation" /> to render shapes to images, which requires loading the file from disk.
    /// </remarks>
    /// <param name="spirePresentation">The presentation.</param>
    /// <param name="index">
    ///     The 1-based index of the slide to extract shapes from. Index can be only in [1, 10] due to
    ///     FreeSpire.Presentation limitations.
    /// </param>
    /// <returns>A dictionary mapping shape IDs to shape image info.</returns>
    public static Dictionary<uint, ImageShapePreview> ExtractAllImageShapes(Presentation spirePresentation,
        int index = 1)
    {
        if (spirePresentation.Slides.Count == 0) return [];

        var slide = spirePresentation.Slides[index + 1];
        Dictionary<uint, ImageShapePreview> shapes = [];
        foreach (var shape in slide.Shapes.ToArray())
        {
            if (shape.IsHidden) continue;
            if (shape is not SlidePicture && shape.Fill.FillType != FillFormatType.Picture) continue;
            using var imageStream = shape.SaveAsImage();
            using var ms = new MemoryStream();
            imageStream.CopyTo(ms);
            shapes.Add(shape.Id, new ImageShapePreview(shape.Id, shape.Name, ms.ToArray()));
        }

        return shapes;
    }
}