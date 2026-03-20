using System.Drawing;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Picture = DocumentFormat.OpenXml.Presentation.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace SlideGenerator.Framework.Slide.Services;

/// Reviewed by @thnhmai06 at 05/03/2026
public static class ShapeService
{
    /// <summary>
    ///     Gets the size of a shape in pixels.
    /// </summary>
    /// <param name="shape">The shape element.</param>
    /// <returns>The size in pixels.</returns>
    public static Size GetSize(this Shape shape)
    {
        var transform = shape.ShapeProperties?.Transform2D;
        var width = (int)((transform?.Extents?.Cx?.Value ?? 0) / 9525);
        var height = (int)((transform?.Extents?.Cy?.Value ?? 0) / 9525);
        return new Size(Math.Max(1, width), Math.Max(1, height));
    }

    /// <summary>
    ///     Gets the size of a picture in pixels.
    /// </summary>
    /// <param name="picture">The picture element.</param>
    /// <returns>The size in pixels.</returns>
    public static Size GetSize(this Picture picture)
    {
        var transform = picture.ShapeProperties?.Transform2D;
        var width = (int)((transform?.Extents?.Cx?.Value ?? 0) / 9525);
        var height = (int)((transform?.Extents?.Cy?.Value ?? 0) / 9525);
        return new Size(Math.Max(1, width), Math.Max(1, height));
    }

    /// <summary>
    ///     Gets the ID of a shape.
    /// </summary>
    /// <param name="shape">The shape element.</param>
    /// <returns>The ID of a shape</returns>
    public static uint? GetId(this Shape shape)
    {
        var nvShapePr = shape.NonVisualShapeProperties;
        return nvShapePr?.NonVisualDrawingProperties?.Id?.Value;
    }

    /// <summary>
    ///     Gets the ID of a picture.
    /// </summary>
    /// <param name="picture">The picture element.</param>
    /// <returns>The ID of a picture</returns>
    public static uint? GetId(this Picture picture)
    {
        var nvPicPr = picture.NonVisualPictureProperties;
        return nvPicPr?.NonVisualDrawingProperties?.Id?.Value;
    }

    /// <param name="slidePart">The slide part to search.</param>
    extension(SlidePart slidePart)
    {
        /// <summary>
        ///     Finds picture elements in a slide by shape ids.
        /// </summary>
        /// <param name="ids">Target shape ids passed as <c>params</c>.</param>
        /// <returns>Matching pictures in slide document order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids" /> is <see langword="null" />.</exception>
        public IReadOnlyList<Picture> FindPicture(params HashSet<uint> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var slide = slidePart.Slide;
            if (slide == null || ids.Count == 0) return [];

            return slide.Descendants<Picture>()
                .Where(picture =>
                {
                    var id = picture.GetId();
                    return id.HasValue && ids.Contains(id.Value);
                })
                .ToList();
        }

        /// <summary>
        ///     Finds shape elements in a slide by shape ids.
        /// </summary>
        /// <param name="ids">Target shape ids passed as <c>params</c>.</param>
        /// <returns>Matching shapes in slide document order.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids" /> is <see langword="null" />.</exception>
        public IReadOnlyList<Shape> FindShape(params HashSet<uint> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var slide = slidePart.Slide;
            if (slide == null || ids.Count == 0) return [];

            return slide.Descendants<Shape>()
                .Where(shape =>
                {
                    var id = shape.GetId();
                    return id.HasValue && ids.Contains(id.Value);
                })
                .ToList();
        }

        /// <summary>
        ///     Gets distinct ordered ids of <see cref="Picture" /> elements in a slide.
        /// </summary>
        /// <returns>Distinct ordered picture shape ids.</returns>
        public HashSet<uint> GetPictureIds()
        {
            var slide = slidePart.Slide;
            if (slide == null) return [];

            return slide.Descendants<Picture>()
                .Select(GetId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .OrderBy(id => id)
                .ToHashSet();
        }

        /// <summary>
        ///     Gets distinct ordered ids of <see cref="Shape" /> elements whose fill is an embedded image.
        /// </summary>
        /// <returns>Distinct ordered image-filled shape ids.</returns>
        public HashSet<uint> GetImageFilledShapeIds()
        {
            var slide = slidePart.Slide;
            if (slide == null) return [];

            return slide.Descendants<Shape>()
                .Where(shape => shape.ShapeProperties?.GetFirstChild<BlipFill>()?.Blip?.Embed != null)
                .Select(GetId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .OrderBy(id => id)
                .ToHashSet();
        }

        /// <summary>
        ///     Gets all distinct shape ids that can be treated as images in a slide.
        ///     This includes <see cref="Picture" /> ids and image-filled <see cref="Shape" /> ids.
        /// </summary>
        /// <returns>Distinct ordered ids from picture elements and image-filled shape elements.</returns>
        public HashSet<uint> GetImageShapeIds()
        {
            return slidePart.GetPictureIds()
                .Concat(slidePart.GetImageFilledShapeIds())
                .Distinct()
                .OrderBy(id => id)
                .ToHashSet();
        }
    }
}