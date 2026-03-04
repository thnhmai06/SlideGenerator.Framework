using SlideGenerator.Framework.Slide.Models;
using Spire.Presentation;
using Spire.Presentation.Drawing;
using SlidePicture = Spire.Presentation.SlidePicture;
using SpirePresentation = Spire.Presentation.Presentation;

namespace SlideGenerator.Framework.Slide.Services.Presentation;

/// Reviewed by @thnhmai06 at 05/03/2026
public static class SpirePresentationService
{
    /// <param name="spirePresentation">The presentation.</param>
    extension(SpirePresentation spirePresentation)
    {
        /// <summary>
        ///     Gets all preview of picture or image-filled shapes from the presentation file.
        /// </summary>
        /// <remarks>
        ///     This method uses <see cref="Presentation" /> to render shapes to images, which requires loading the file from disk.
        /// </remarks>
        /// <param name="slideIndex">
        ///     The 1-based index of the slide to extract shapes from. Index can be only in [1, 10] due to
        ///     FreeSpire.Presentation limitations.
        /// </param>
        /// <returns>A dictionary mapping shape IDs to shape image info.</returns>
        public IReadOnlyDictionary<uint, ObjectPreview> ExtractPreviewImageShapes(int slideIndex)
        {
            Dictionary<uint, ObjectPreview> shapes = [];
            if (spirePresentation.Slides.Count == 0) return shapes;

            var slide = spirePresentation.Slides[slideIndex + 1];
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
        /// <returns>
        ///     A list of <see cref="ObjectPreview" /> instances, each containing the slide ID, name, and a byte array
        ///     representing the preview image for each slide. The list will be empty if the presentation contains no slides.
        /// </returns>
        public IReadOnlyList<ObjectPreview> ExtractPreviewSlides()
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
}