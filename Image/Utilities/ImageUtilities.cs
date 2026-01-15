using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Saliency;
using SlideGenerator.Framework.Image.Exceptions;

namespace SlideGenerator.Framework.Image.Utilities;

public static class ImageUtilities
{
    /// <summary>
    ///     Get the largest size that has the same aspect ratio with the target size and fits within the original size.
    /// </summary>
    /// <param name="original">The original size.</param>
    /// <param name="target">The target size.</param>
    /// <returns>The largest size that has the same aspect ratio with the target size and fits within the original size.</returns>
    public static Size GetMaxAspectSize(Size original, Size target)
    {
        var originalAspect = original.Width / (double)original.Height;
        var targetAspect = target.Width / (double)target.Height;

        int width, height;
        if (originalAspect >= targetAspect)
        {
            height = original.Height;
            width = (int)Math.Round(height * targetAspect);
        }
        else
        {
            width = original.Width;
            height = (int)Math.Round(width / targetAspect);
        }

        width = Math.Min(width, original.Width);
        height = Math.Min(height, original.Height);
        return new Size(width, height);
    }

    /// <summary>
    ///     Crops an image to the specified region of interest.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="rect">The region of interest to crop to.</param>
    public static void Crop(Models.Image image, Rectangle rect)
    {
        var croppedMat = new Mat(image.Mat, rect);
        var cloned = croppedMat.Clone();

        image.Mat.Dispose();
        image.Mat = cloned;
        croppedMat.Dispose();
    }

    /// <summary>
    ///     Resizes the specified image to the given dimensions in place.
    /// </summary>
    /// <param name="image">The image to resize (modified in place).</param>
    /// <param name="size">The size to resize to.</param>
    public static void Resize(Models.Image image, Size size)
    {
        var resizedMat = new Mat();
        CvInvoke.Resize(image.Mat, resizedMat, size, 0, 0, Inter.Area);

        image.Mat.Dispose();
        image.Mat = resizedMat;
    }

    /// <summary>
    ///     Computes a normalized saliency map for the specified image using the spectral residual method.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <returns>Normalized saliency map (value from 0 to 1).</returns>
    public static Mat ComputeSaliency(Models.Image image)
    {
        using var saliency = new StaticSaliencySpectralResidual();
        using var saliencyMap = new Mat();

        var ok = saliency.Compute(image.Mat, saliencyMap);
        if (!ok) throw new ComputeSaliencyFailed(image.SourceName);

        using var outMap = new Mat();
        saliencyMap.ConvertTo(outMap, DepthType.Cv32F);
        CvInvoke.Normalize(outMap, outMap, 0, 1, NormType.MinMax);

        return outMap.Clone();
    }

    /// <summary>
    ///     Calculates the visual center of a shape represented by a mask image.
    /// </summary>
    /// <remarks>
    ///     The visual center (not centroid) is determined using a distance transform, which finds the point farthest from the
    ///     mask boundary.
    ///     The input can be grayscale or color; it is converted to a single-channel binary image internally.
    /// </remarks>
    /// <param name="image">
    ///     Mask image (grayscale or color) where non-zero pixels represent the shape. Must not be null or
    ///     empty.
    /// </param>
    /// <returns>Coordinates of the visual center (pixel farthest from edges).</returns>
    public static Point ComputeVisualCenter(Models.Image image)
    {
        var mat = image.Mat;
        if (mat == null || mat.IsEmpty)
            throw new ArgumentException("Image is empty", nameof(image));

        // convert to only 1 channel binary image
        using var gray = mat.NumberOfChannels > 1 ? new Mat() : mat.Clone();
        if (mat.NumberOfChannels > 1) CvInvoke.CvtColor(mat, gray, ColorConversion.Bgr2Gray);
        using var binary = new Mat();
        CvInvoke.Threshold(gray, binary, 127, 255, ThresholdType.Binary);

        if (CvInvoke.CountNonZero(binary) == 0)
            throw new ArgumentException("Image does not contains foreground pixels", nameof(image));

        // Each pixel inside the shape is assigned a value representing the distance to the nearest edge
        using var distTransform = new Mat();
        CvInvoke.DistanceTransform(binary, distTransform, null, DistType.L2, 3);

        // Determine the center by finding the pixel with the maximum value (i.e., farthest from edges)
        double minVal = 0, maxVal = 0;
        Point minLoc = new(), maxLoc = new();
        CvInvoke.MinMaxLoc(distTransform, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        return maxLoc;
    }

    /// <summary>
    ///     Clamps the specified rectangle so that it fits entirely within the bounds of the given border rectangle.
    /// </summary>
    /// <remarks>
    ///     If the input rectangle is larger than the border in either dimension, the returned rectangle
    ///     will be resized to match the border's corresponding dimension. The position is adjusted as needed to ensure the
    ///     rectangle remains fully within the border.
    /// </remarks>
    /// <param name="rect">The rectangle to be clamped within the border.</param>
    /// <param name="border">
    ///     The rectangle that defines the border limits. The returned rectangle will not extend outside this
    ///     area.
    /// </param>
    /// <returns>
    ///     A rectangle with the same size as the input rectangle, unless it exceeds the border's dimensions, in which case
    ///     its size and position are adjusted to fit entirely within the border.
    /// </returns>
    public static Rectangle ClampToBorder(Rectangle rect, Rectangle border)
    {
        var x = rect.X;
        var y = rect.Y;
        var w = rect.Width;
        var h = rect.Height;

        if (w > border.Width)
        {
            w = border.Width;
            x = border.X;
        }

        if (h > border.Height)
        {
            h = border.Height;
            y = border.Y;
        }

        if (x < border.Left) x = border.Left;
        if (y < border.Top) y = border.Top;
        if (x + w > border.Right) x = border.Right - w;
        if (y + h > border.Bottom) y = border.Bottom - h;

        return new Rectangle(x, y, w, h);
    }

    /// <summary>
    ///     Clamps the specified point so that its coordinates lie within the bounds of the given rectangle.
    /// </summary>
    /// <remarks>
    ///     If the point's coordinates are already within the rectangle, the original point is returned
    ///     unchanged. The rectangle's right and bottom edges are considered exclusive, matching standard .NET rectangle
    ///     behavior.
    /// </remarks>
    /// <param name="point">The point to clamp to the rectangle's borders.</param>
    /// <param name="border">
    ///     The rectangle that defines the bounding area. The point will be clamped to remain within this
    ///     rectangle.
    /// </param>
    /// <returns>A new point whose X and Y coordinates are constrained to the borders of the specified rectangle.</returns>
    public static Point ClampToBorder(Point point, Rectangle border)
    {
        var x = Math.Clamp(point.X, border.Left, border.Right - 1);
        var y = Math.Clamp(point.Y, border.Top, border.Bottom - 1);

        return new Point(x, y);
    }
}