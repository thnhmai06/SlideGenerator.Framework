using System.Drawing;
using Picture = DocumentFormat.OpenXml.Presentation.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace SlideGenerator.Framework.Slide;

public static class ShapeService
{
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