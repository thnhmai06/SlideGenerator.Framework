using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using ImageMagick;
using SlideGenerator.Framework.Image.Exceptions;

namespace SlideGenerator.Framework.Image.Models;

/// <summary>
///     Represents an image with support for manipulation and saving.
/// </summary>
public sealed class Image : IDisposable, ICloneable
{
    private bool _disposed;

    /// <summary>
    ///     Creates an Image instance from a file.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="ReadImageFailed">Thrown when the image cannot be read.</exception>
    public Image(string filePath)
    {
        SourceName = filePath;
        try
        {
            using var magickImage = new MagickImage(filePath);
            var pixels = magickImage.GetPixels();
            var bytes = pixels.ToByteArray(PixelMapping.BGR) ?? throw new ReadImageFailed(filePath);
            Mat = new Mat((int)magickImage.Height, (int)magickImage.Width, DepthType.Cv8U, 3);
            Marshal.Copy(bytes, 0, Mat.DataPointer, bytes.Length);
            if (Mat.IsEmpty)
                throw new ReadImageFailed(filePath);
        }
        catch (ReadImageFailed)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ReadImageFailed(filePath, ex);
        }
    }

    /// <summary>
    ///     Creates an ImageData instance from a byte array.
    /// </summary>
    /// <param name="bytes">Image data as byte array.</param>
    /// <param name="sourceName">Optional name for error messages.</param>
    /// <exception cref="ReadImageFailed">Thrown when the image cannot be read.</exception>
    public Image(byte[] bytes, string sourceName = "memory")
    {
        SourceName = sourceName;

        try
        {
            using var magickImage = new MagickImage(bytes);
            var pixels = magickImage.GetPixels();
            var bgrBytes = pixels.ToByteArray(PixelMapping.BGR) ?? throw new ReadImageFailed(sourceName);
            Mat = new Mat((int)magickImage.Height, (int)magickImage.Width, DepthType.Cv8U, 3);
            Marshal.Copy(bgrBytes, 0, Mat.DataPointer, bgrBytes.Length);
            if (Mat.IsEmpty)
                throw new ReadImageFailed(sourceName);
        }
        catch (ReadImageFailed)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ReadImageFailed(sourceName, ex);
        }
    }

    /// <summary>
    ///     Gets the file path of the source image. If loaded from memory, this is a descriptive name.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    ///     Gets or sets the underlying OpenCV Mat object.
    /// </summary>
    public Mat Mat { get; internal set; }

    /// <summary>
    ///     Gets the size of the image.
    /// </summary>
    public Size Size => new(Mat.Width, Mat.Height);

    public object Clone()
    {
        return new Image(ToByteArray(), SourceName);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Mat.Dispose();
    }

    /// <summary>
    ///     Converts the image to a byte array in PNG format.
    /// </summary>
    /// <returns>The image data as a byte array.</returns>
    public byte[] ToByteArray()
    {
        using var buffer = new VectorOfByte();
        CvInvoke.Imencode(".png", Mat, buffer);
        return buffer.ToArray();
    }
}