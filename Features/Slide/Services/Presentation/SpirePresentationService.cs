using SlideGenerator.Framework.Slide.Models;
using Spire.Presentation;
using Spire.Presentation.Drawing;

namespace SlideGenerator.Framework.Slide.Services;

public static class SpirePresentationService
{
    /// <summary>
    ///     Opens a Spire Presentation file from the specified path and returns a Presentation instance for further
    ///     manipulation.
    /// </summary>
    /// <remarks>
    ///     If the file is opened in read-only mode, changes to the presentation will not be saved to the
    ///     original file. Ensure that the file is not in use by another process when opening in write mode.
    /// </remarks>
    /// <param name="filePath">The full path to the presentation file to open. The file must exist and be accessible.</param>
    /// <param name="readOnly">true to open the file in read-only mode; otherwise, false. Defaults to true.</param>
    /// <returns>A Presentation object representing the opened presentation file.</returns>
    public static Presentation OpenSpirePresentation(string filePath, bool readOnly = true)
    {
        using var fs = new FileStream(
            filePath, FileMode.Open,
            readOnly ? FileAccess.ReadWrite : FileAccess.Read,
            readOnly ? FileShare.ReadWrite : FileShare.Read);
        return new Presentation(fs, FileFormat.Auto);
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
    public static Dictionary<uint, ObjectPreview> ExtractAllImageShapes(Presentation spirePresentation,
        int index = 1)
    {
        if (spirePresentation.Slides.Count == 0) return [];

        var slide = spirePresentation.Slides[index + 1];
        Dictionary<uint, ObjectPreview> shapes = [];
        foreach (var shape in slide.Shapes.ToArray())
        {
            if (shape.IsHidden) continue;
            if (shape is not SlidePicture && shape.Fill.FillType != FillFormatType.Picture) continue;
            using var imageStream = shape.SaveAsImage();
            using var ms = new MemoryStream();
            imageStream.CopyTo(ms);
            shapes.Add(shape.Id, new ObjectPreview(shape.Id, shape.Name, ms.ToArray()));
        }

        return shapes;
    }

    /// <summary>
    ///     Extracts preview images for all slides in the specified presentation.
    /// </summary>
    /// <remarks>
    ///     Each preview corresponds to a single slide in the order they appear in the presentation. The
    ///     preview image is generated using the slide's current content and formatting.
    ///     Due to limitations in FreeSpire.Presentation, this method can only process presentations with up to first 10
    ///     slides.
    /// </remarks>
    /// <param name="spirePresentation">The presentation from which to extract slide previews. Cannot be null.</param>
    /// <returns>
    ///     A list of <see cref="ObjectPreview" /> instances, each containing the slide ID, name, and a byte array
    ///     representing the preview image for each slide. The list will be empty if the presentation contains no slides.
    /// </returns>
    public static List<ObjectPreview> ExtractAllSlidesPreviews(Presentation spirePresentation)
    {
        List<ObjectPreview> previews = [];
        foreach (var slide in spirePresentation.Slides.ToArray())
        {
            using var imageStream = slide.SaveAsImage();
            using var ms = new MemoryStream();
            imageStream.CopyTo(ms);
            previews.Add(new ObjectPreview(slide.SlideID, slide.Name, ms.ToArray()));
        }

        return previews;
    }
}