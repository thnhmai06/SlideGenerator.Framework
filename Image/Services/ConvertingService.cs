using ImageMagick;
using OpenCvSharp;

namespace SlideGenerator.Framework.Image.Services;

/// Reviewed by @thnhmai06 at 05/03/2026
public static class ConvertingService
{
    /// <summary>
    ///     Converts a <see cref="MagickImage" /> to an OpenCV <see cref="Mat" />.
    /// </summary>
    /// <param name="image">The source <see cref="MagickImage" /> to convert.</param>
    /// <param name="mapping">The mapping of the pixels (e.g. RGB/RGBA/ARGB).</param>
    /// <returns>
    ///     A <see cref="Mat" /> containing the image data, or <see langword="null" /> if the conversion fails.
    /// </returns>
    public static Mat? ConvertToMat(this MagickImage image, PixelMapping mapping = PixelMapping.BGR)
    {
        var pixels = image.GetPixels();
        var bytes = pixels.ToByteArray(mapping);
        return bytes == null ? null : Mat.FromImageData(bytes);
    }
}