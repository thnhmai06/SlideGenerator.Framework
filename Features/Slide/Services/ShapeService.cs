using System.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Picture = DocumentFormat.OpenXml.Presentation.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace SlideGenerator.Framework.Features.Slide.Services;

public static class ShapeService
{
    /// <summary>
    ///     Finds a picture element in a slide by shape id.
    /// </summary>
    /// <param name="slidePart">The slide part to search.</param>
    /// <param name="shapeId">Target shape id.</param>
    /// <returns>The matching picture if found; otherwise <see langword="null" />.</returns>
    public static Picture? FindPictureById(SlidePart slidePart, uint shapeId)
    {
        var slide = slidePart.Slide;
        return slide?.Descendants<Picture>()
            .FirstOrDefault(p => p.NonVisualPictureProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
    }

    /// <summary>
    ///     Finds a shape element in a slide by shape id.
    /// </summary>
    /// <param name="slidePart">The slide part to search.</param>
    /// <param name="shapeId">Target shape id.</param>
    /// <returns>The matching shape if found; otherwise <see langword="null" />.</returns>
    public static Shape? FindShapeById(SlidePart slidePart, uint shapeId)
    {
        var slide = slidePart.Slide;
        return slide?.Descendants<Shape>()
            .FirstOrDefault(s => s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value == shapeId);
    }

    /// <summary>
    ///     Gets all distinct image-capable shape ids in a slide.
    /// </summary>
    /// <param name="slidePart">The slide part to inspect.</param>
    /// <returns>Distinct ordered ids of picture shapes.</returns>
    public static IReadOnlyList<uint> GetImageShapeIds(SlidePart slidePart)
    {
        var slide = slidePart.Slide;
        if (slide == null) return [];

        return slide.Descendants<Picture>()
            .Select(GetId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
    }

    /// <summary>
    ///     Gets the size of a shape in pixels.
    /// </summary>
    /// <param name="shape">The shape element.</param>
    /// <returns>The size in pixels.</returns>
    public static Size GetShapeSize(Shape shape)
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
    public static Size GetPictureSize(Picture picture)
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
    public static uint? GetId(Shape shape)
    {
        var nvShapePr = shape.NonVisualShapeProperties;
        return nvShapePr?.NonVisualDrawingProperties?.Id?.Value;
    }

    /// <summary>
    ///     Gets the ID of a picture.
    /// </summary>
    /// <param name="picture">The picture element.</param>
    /// <returns>The ID of a picture</returns>
    public static uint? GetId(Picture picture)
    {
        var nvPicPr = picture.NonVisualPictureProperties;
        return nvPicPr?.NonVisualDrawingProperties?.Id?.Value;
    }
}