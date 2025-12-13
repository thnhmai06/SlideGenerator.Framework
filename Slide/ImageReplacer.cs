using System.Drawing;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using SlideGenerator.Core.Slide.Exceptions;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using Picture = DocumentFormat.OpenXml.Drawing.Picture;

namespace SlideGenerator.Core.Slide;

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
    /// <param name="imageStream">Stream containing the new image data (PNG format).</param>
    public static void ReplaceImage(SlidePart slidePart, Picture picture, Stream imageStream)
    {
        var imgPart = slidePart.AddImagePart(ImagePartType.Png);
        imgPart.FeedData(imageStream);
        var rId = slidePart.GetIdOfPart(imgPart);

        var blip = picture.Descendants<Blip>().FirstOrDefault();
        if (blip is null)
            throw new ImageNotFoundInShapeException(GetPictureId(picture), "No Blip element found.");

        var embed = blip.Embed;
        if (embed is null)
            throw new ImageNotFoundInShapeException(GetPictureId(picture), "No Embed reference in Blip.");

        embed.Value = rId;
        slidePart.Slide.Save();
    }

    /// <summary>
    ///     Replaces the image in a Shape element (shape filled with image).
    /// </summary>
    /// <param name="slidePart">The slide part containing the shape.</param>
    /// <param name="shape">The shape element to update.</param>
    /// <param name="imageStream">Stream containing the new image data (PNG format).</param>
    public static void ReplaceImage(SlidePart slidePart, Shape shape, Stream imageStream)
    {
        var imgPart = slidePart.AddImagePart(ImagePartType.Png);
        imgPart.FeedData(imageStream);
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
            throw new ImageNotFoundInShapeException(GetShapeId(shape), "No Blip element found in shape fill.");

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
        if (transform?.Extents == null)
            return new Size(400, 300);

        // EMUs to pixels (9525 EMUs per pixel at 96 DPI)
        var width = (int)((transform.Extents.Cx?.Value ?? 3810000) / 9525);
        var height = (int)((transform.Extents.Cy?.Value ?? 2857500) / 9525);
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
        if (transform?.Extents == null)
            return new Size(400, 300);

        // EMUs to pixels (9525 EMUs per pixel at 96 DPI)
        var width = (int)((transform.Extents.Cx?.Value ?? 3810000) / 9525);
        var height = (int)((transform.Extents.Cy?.Value ?? 2857500) / 9525);
        return new Size(Math.Max(1, width), Math.Max(1, height));
    }

    private static string GetPictureId(Picture picture)
    {
        var nvPicPr = picture.NonVisualPictureProperties;
        return nvPicPr?.NonVisualDrawingProperties?.Id?.Value.ToString() ?? "Unknown";
    }

    private static string GetShapeId(Shape shape)
    {
        var nvShapePr = shape.NonVisualShapeProperties;
        return nvShapePr?.NonVisualDrawingProperties?.Id?.Value.ToString() ?? "Unknown";
    }
}