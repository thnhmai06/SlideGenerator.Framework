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
public sealed class ImageData : IDisposable, ICloneable
{
    private bool _disposed;

    /// <summary>
    ///     Creates an ImageData instance from a file.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="ImageReadException">Thrown when the image cannot be read.</exception>
    public ImageData(string filePath)
    {
        FilePath = filePath;
        if (!File.Exists(FilePath))
            throw new FileNotFoundException("Image file not found.", filePath);

        try
        {
            using var magickImage = new MagickImage(filePath);
            var pixels = magickImage.GetPixels();
            var bytes = pixels.ToByteArray(PixelMapping.BGR);
            if (bytes == null)
                throw new ImageReadException(filePath);

            Mat = new Mat((int)magickImage.Height, (int)magickImage.Width, DepthType.Cv8U, 3);
            Marshal.Copy(bytes, 0, Mat.DataPointer, bytes.Length);
            if (Mat.IsEmpty)
                throw new ImageReadException(filePath);
        }
        catch (ImageReadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ImageReadException(filePath, ex);
        }
    }

    /// <summary>
    ///     Creates an ImageData instance from a byte array.
    /// </summary>
    /// <param name="bytes">Image data as byte array.</param>
    /// <param name="sourceName">Optional name for error messages.</param>
    /// <exception cref="ImageReadException">Thrown when the image cannot be read.</exception>
    public ImageData(byte[] bytes, string sourceName = "memory")
    {
        FilePath = sourceName;

        try
        {
            using var magickImage = new MagickImage(bytes);
            var pixels = magickImage.GetPixels();
            var bgrBytes = pixels.ToByteArray(PixelMapping.BGR);
            if (bgrBytes == null)
                throw new ImageReadException(sourceName);

            Mat = new Mat((int)magickImage.Height, (int)magickImage.Width, DepthType.Cv8U, 3);
            Marshal.Copy(bgrBytes, 0, Mat.DataPointer, bgrBytes.Length);
            if (Mat.IsEmpty)
                throw new ImageReadException(sourceName);
        }
        catch (ImageReadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ImageReadException(sourceName, ex);
        }
    }

    /// <summary>
    ///     Gets the file path of the source image. If loaded from memory, this is a descriptive name.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     Gets or sets the underlying OpenCV Mat object.
    /// </summary>
    public Mat Mat { get; private set; }

    /// <summary>
    ///     Gets the size of the image.
    /// </summary>
    public Size Size => new(Mat.Width, Mat.Height);

    public object Clone()
    {
        return new ImageData(ToByteArray(), FilePath);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Mat.Dispose();
    }

    /// <summary>
    ///     Saves the image to the specified file path in PNG format.
    /// </summary>
    /// <param name="savePath">The path to save the image to. If null, saves to the original path.</param>
    public void Save(string? savePath = null)
    {
        savePath ??= FilePath;

        using var buffer = new VectorOfByte();
        CvInvoke.Imencode(".png", Mat, buffer);
        var bytes = buffer.ToArray();

        using var magick = new MagickImage(bytes);
        magick.Write(savePath);
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

    /// <summary>
    ///     Creates a cropped copy of this image.
    /// </summary>
    /// <param name="roi">The region of interest to crop.</param>
    /// <returns>A new ImageData instance containing the cropped image.</returns>
    public ImageData Crop(Rectangle roi)
    {
        var croppedMat = new Mat(Mat, roi);
        var result = new ImageData(ToByteArray(), FilePath)
        {
            Mat = croppedMat.Clone()
        };
        croppedMat.Dispose();
        return result;
    }

    /// <summary>
    ///     Crops this image in place.
    /// </summary>
    /// <param name="roi">The region of interest to crop.</param>
    public void CropInPlace(Rectangle roi)
    {
        var croppedMat = new Mat(Mat, roi);
        var cloned = croppedMat.Clone();

        Mat.Dispose();
        Mat = cloned;
        croppedMat.Dispose();
    }

    /// <summary>
    ///     Create a resized copy of the underlying image.
    /// </summary>
    /// <remarks>
    ///     The original image remains unchanged. The resizing operation uses area-based interpolation,
    ///     which is generally preferred for image shrinking to preserve quality.
    /// </remarks>
    /// <param name="size">The new size</param>
    /// <returns>A new ImageData instance containing the resized image.</returns>
    public ImageData Resize(Size size)
    {
        var res = new Mat();
        CvInvoke.Resize(Mat, res, size, 0, 0, Inter.Area);
        var result = new ImageData(ToByteArray(), FilePath)
        {
            Mat = res
        };
        return result;
    }

    /// <summary>
    ///     Resize this image in place.
    /// </summary>
    /// <remarks>
    ///     This method modifies the current image by resizing it and discarding the original data. After
    ///     calling this method, any references to the previous image data become invalid.
    /// </remarks>
    /// <param name="size">The new size</param>
    public void ResizeInPlace(Size size)
    {
        var resizedMat = new Mat();
        CvInvoke.Resize(Mat, resizedMat, size, 0, 0, Inter.Area);

        Mat.Dispose();
        Mat = resizedMat;
    }
}