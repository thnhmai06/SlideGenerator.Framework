using OpenCvSharp;
using Point = System.Drawing.Point;

namespace SlideGenerator.Framework.Image.Services;

/// <summary>
///     Provides static methods for computing image saliency maps and determining the visual center of shapes within
///     images.
/// </summary>
/// Reviewed by @thnhmai06 at 01/03/2026 02:11:50 GMT+7
public static class ComputingService
{
    /// <summary>
    ///     Computes a normalized saliency map for the specified image using a simple spectral residual-like method.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <returns>Normalized saliency map (value from 0 to 1). If computation fails, returns null.</returns>
    public static Mat? ComputeSaliency(Mat image)
    {
        if (image.Empty()) return null;

        try
        {
            // Convert to grayscale if needed
            using var gray = image.Channels() > 1 ? new Mat() : image.Clone();
            if (image.Channels() > 1) Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Convert to float
            using var floatImg = new Mat();
            gray.ConvertTo(floatImg, MatType.CV_32F, 1.0 / 255.0);

            // Apply Laplacian as a simple saliency detector (alternative to spectral residual)
            using var laplacian = new Mat();
            Cv2.Laplacian(floatImg, laplacian, MatType.CV_32F, 5);

            // Get absolute values
            using var absLaplacian = new Mat();
            Cv2.ConvertScaleAbs(laplacian, absLaplacian);

            // Normalize to 0-1 range
            using var normalized = new Mat();
            Cv2.Normalize(absLaplacian, normalized, 0, 1, NormTypes.MinMax);

            return normalized.Clone();
        }
        catch
        {
            return null;
        }
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
    /// <returns>
    ///     Coordinates of the visual center (pixel farthest from edges). If the image is empty or has no non-zero pixels,
    ///     returns null.
    /// </returns>
    public static Point? ComputeVisualCenter(Mat image)
    {
        if (image.Empty()) return null;

        // convert to only 1 channel binary image
        using var gray = image.Channels() > 1 ? new Mat() : image.Clone();
        if (image.Channels() > 1) Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        using var binary = new Mat();
        Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Binary);
        if (Cv2.CountNonZero(binary) == 0) return null;

        // Each pixel inside the shape is assigned a value representing the distance to the nearest edge
        using var distTransform = new Mat();
        Cv2.DistanceTransform(binary, distTransform, DistanceTypes.L2, DistanceTransformMasks.Precise);

        // Determine the center by finding the pixel with the maximum value (i.e., farthest from edges)
        Cv2.MinMaxLoc(distTransform, out _, out _, out _, out var maxLoc);

        // Convert OpenCvSharp.Point to System.Drawing.Point
        return new Point(maxLoc.X, maxLoc.Y);
    }
}