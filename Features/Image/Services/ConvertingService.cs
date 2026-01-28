using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using ImageMagick;

namespace SlideGenerator.Framework.Image;

public static class ConvertingService
{
    /// <summary>
    ///     Converts a MagickImage to an OpenCV Mat in BGR format.
    /// </summary>
    /// <remarks>
    ///     The returned Mat uses 8-bit unsigned depth and 3 channels (BGR). The caller is responsible
    ///     for disposing the returned Mat when it is no longer needed.
    /// </remarks>
    /// <param name="image">The source <see cref="MagickImage" /> to convert. Must not be <see langword="null" />.</param>
    /// <returns>
    ///     A <see cref="Mat" /> containing the image data in BGR channel order, or <see langword="null" /> if the
    ///     conversion fails.
    /// </returns>
    public static Mat? ConvertImageToMat(MagickImage image)
    {
        var pixels = image.GetPixels();
        var bytes = pixels.ToByteArray(PixelMapping.BGR);
        if (bytes == null) return null;

        var mat = new Mat((int)image.Height, (int)image.Width, DepthType.Cv8U, 3);
        Marshal.Copy(bytes, 0, mat.DataPointer, bytes.Length);
        return mat;
    }

    /// <summary>
    ///     Encodes an OpenCV Mat image to a byte array using the specified file format extension.
    /// </summary>
    /// <remarks>
    ///     The supported file extensions depend on the codecs available in the underlying OpenCV
    ///     installation. If an unsupported extension is provided, the method may throw an exception.
    /// </remarks>
    /// <param name="mat">The Mat image to encode. Must not be null.</param>
    /// <param name="ext">
    ///     The file extension that specifies the image format (for example, ".png" or ".jpg"). Defaults to ".png" if not
    ///     specified.
    /// </param>
    /// <param name="parameters">Optional encoding parameters as key-value pairs.</param>
    /// <returns>A byte array containing the encoded image data in the specified format.</returns>
    public static byte[] ConvertMatToImage(Mat mat, string ext = ".png",
        params KeyValuePair<ImwriteFlags, int>[] parameters)
    {
        using var buffer = new VectorOfByte();
        CvInvoke.Imencode(ext, mat, buffer, parameters);
        return buffer.ToArray();
    }
}