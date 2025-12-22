using System.Drawing;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using SlideGenerator.Framework.Slide.Exceptions;
using Picture = DocumentFormat.OpenXml.Presentation.Picture;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace SlideGenerator.Framework.Slide;

/// <summary>
///     Provides image replacement functionality for slides.
/// </summary>
public static class ImageReplacer
{
    /// <summary>
    ///     Replaces the image in a Picture element.
    /// </summary>
    /// <param name="slidePart">The slide part containing the picture.</param>
    /// <param name="picture">The picture element to update.</param>
    /// <param name="pngStream">Stream containing the new image data (PNG format).</param>
    public static void ReplaceImage(SlidePart slidePart, Picture picture, Stream pngStream)
    {
        var imgPart = slidePart.AddImagePart(ImagePartType.Png);
        imgPart.FeedData(pngStream);
        var rId = slidePart.GetIdOfPart(imgPart);

        var blip = picture.Descendants<Blip>().FirstOrDefault();
        if (blip is null)
            throw new ShapeDoesNotHaveImage(GetPictureId(picture), "No Blip element found.");

        var embed = blip.Embed;
        if (embed is null)
            throw new ShapeDoesNotHaveImage(GetPictureId(picture), "No Embed reference in Blip.");

        embed.Value = rId;
        slidePart.Slide.Save();
    }

    /// <summary>
    ///     Replaces the image in a Shape element (shape filled with image).
    /// </summary>
    /// <param name="slidePart">The slide part containing the shape.</param>
    /// <param name="shape">The shape element to update.</param>
    /// <param name="pngStream">Stream containing the new image data (PNG format).</param>
    public static void ReplaceImage(SlidePart slidePart, Shape shape, Stream pngStream)
    {
        var imgPart = slidePart.AddImagePart(ImagePartType.Png);
        imgPart.FeedData(pngStream);
        var rId = slidePart.GetIdOfPart(imgPart);

        var blipFill = shape.ShapeProperties?.GetFirstChild<BlipFill>();
        var blip = blipFill?.Blip;

        // Try fill properties if not found directly
        if (blip?.Embed == null)
        {
            var fillProps = shape.ShapeProperties?.GetFirstChild<FillProperties>();
            blipFill = fillProps?.GetFirstChild<BlipFill>();
            blip = blipFill?.Blip;
        }

        if (blip?.Embed == null)
            throw new ShapeDoesNotHaveImage(GetShapeId(shape), "No Blip element found in shape fill.");

        blip.Embed.Value = rId;
        slidePart.Slide.Save();
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

    private static string GetShapeId(Shape shape)
    {
        var nvShapePr = shape.NonVisualShapeProperties;
        return nvShapePr?.NonVisualDrawingProperties?.Id?.Value.ToString() ?? "Unknown";
    }

    private static string GetPictureId(Picture picture)
    {
        var nvPicPr = picture.NonVisualPictureProperties;
        return nvPicPr?.NonVisualDrawingProperties?.Id?.Value.ToString() ?? "Unknown";
    }
}