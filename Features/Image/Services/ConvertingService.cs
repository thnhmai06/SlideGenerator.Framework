using ImageMagick;
using OpenCvSharp;

namespace SlideGenerator.Framework.Features.Image.Services;

/// Reviewed by @thnhmai06 at 01 / 03 / 2026 11:47:22 GMT+7
public static class ConvertingService
{
    /// <summary>
    ///     Converts a <see cref="MagickImage"/> to an OpenCV <see cref="Mat"/>.
    /// </summary>
    /// <param name="image">The source <see cref="MagickImage" /> to convert.</param>
    /// <param name="mapping">The mapping of the pixels (e.g. RGB/RGBA/ARGB).</param>
    /// <returns>
    ///     A <see cref="Mat" /> containing the image data, or <see langword="null" /> if the conversion fails.
    /// </returns>
    public static Mat? ConvertImageToMat(MagickImage image, PixelMapping mapping = PixelMapping.BGR)
    {
        var pixels = image.GetPixels();
        var bytes = pixels.ToByteArray(mapping);
        return bytes == null ? null : Mat.FromImageData(bytes);
    }
}