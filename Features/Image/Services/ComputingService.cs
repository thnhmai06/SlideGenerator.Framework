using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Saliency;

namespace SlideGenerator.Framework.Image;

/// <summary>
///     Provides static methods for computing image saliency maps and determining the visual center of shapes within
///     images.
/// </summary>
public static class ComputingService
{
    /// <summary>
    ///     Computes a normalized saliency map for the specified image using the spectral residual method.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <returns>Normalized saliency map (value from 0 to 1). If computation fails, returns null.</returns>
    public static Mat? ComputeSaliency(Mat image)
    {
        using var saliency = new StaticSaliencySpectralResidual();
        using var saliencyMap = new Mat();

        var ok = saliency.Compute(image, saliencyMap);
        if (!ok) return null;

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
    /// <returns>
    ///     Coordinates of the visual center (pixel farthest from edges). If the image is empty or has no non-zero pixels,
    ///     returns null.
    /// </returns>
    public static Point? ComputeVisualCenter(Mat image)
    {
        if (image.IsEmpty) return null;

        // convert to only 1 channel binary image
        using var gray = image.NumberOfChannels > 1 ? new Mat() : image.Clone();
        if (image.NumberOfChannels > 1) CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
        using var binary = new Mat();
        CvInvoke.Threshold(gray, binary, 127, 255, ThresholdType.Binary);
        if (CvInvoke.CountNonZero(binary) == 0) return null;

        // Each pixel inside the shape is assigned a value representing the distance to the nearest edge
        using var distTransform = new Mat();
        CvInvoke.DistanceTransform(binary, distTransform, null, DistType.L2, 3);

        // Determine the center by finding the pixel with the maximum value (i.e., farthest from edges)
        double minVal = 0, maxVal = 0;
        Point minLoc = new(), maxLoc = new();
        CvInvoke.MinMaxLoc(distTransform, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        return maxLoc;
    }
}