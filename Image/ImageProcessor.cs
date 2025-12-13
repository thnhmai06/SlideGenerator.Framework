using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Saliency;
using SlideGenerator.Core.Image.Enums;
using SlideGenerator.Core.Image.Exceptions;
using SlideGenerator.Core.Image.Models;

namespace SlideGenerator.Core.Image;

/// <summary>
///     Provides image processing operations including ROI detection and cropping.
/// </summary>
public static class ImageProcessor
{
    /// <summary>
    ///     Gets the region of interest (ROI) for cropping an image to a target size.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <returns>A rectangle representing the optimal crop region.</returns>
    public static Rectangle GetRoi(ImageData image, RoiType roiType, Size targetSize)
    {
        return roiType switch
        {
            RoiType.Prominent => GetProminentRoi(image, targetSize).Roi,
            RoiType.Center => GetCenterRoi(image, targetSize),
            _ => throw new ArgumentOutOfRangeException(nameof(roiType), roiType, null)
        };
    }

    /// <summary>
    ///     Crops an image to the specified region of interest.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="roi">The region of interest to crop to.</param>
    public static void Crop(ImageData image, Rectangle roi)
    {
        image.CropInPlace(roi);
    }

    /// <summary>
    ///     Crops an image to the optimal region based on the specified ROI type.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <param name="targetSize">The target size for cropping.</param>
    public static void CropToRoi(ImageData image, RoiType roiType, Size targetSize)
    {
        var roi = GetRoi(image, roiType, targetSize);
        Crop(image, roi);
    }

    /// <summary>
    ///     Creates a cropped copy of an image based on the specified ROI type.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <returns>A new cropped image.</returns>
    public static ImageData CropToRoiCopy(ImageData image, RoiType roiType, Size targetSize)
    {
        var roi = GetRoi(image, roiType, targetSize);
        return image.Crop(roi);
    }

    /// <summary>
    ///     Computes a normalized saliency map for the specified image using the spectral residual method.
    /// </summary>
    private static Mat ComputeSaliency(ImageData image)
    {
        using var saliency = new StaticSaliencySpectralResidual();
        using var saliencyMap = new Mat();

        var ok = saliency.Compute(image.Mat, saliencyMap);
        if (!ok) throw new SaliencyComputationException(image.FilePath);

        using var outMap = new Mat();
        saliencyMap.ConvertTo(outMap, DepthType.Cv32F);
        CvInvoke.Normalize(outMap, outMap, 0, 1, NormType.MinMax);

        return outMap.Clone();
    }

    /// <summary>
    ///     Finds the best crop position based on saliency (prominent area).
    ///     Uses median of saliency values in each possible crop region.
    /// </summary>
    private static (Rectangle Roi, double SaliencyValue) GetProminentRoi(ImageData image, Size size)
    {
        using var saliencyMap = ComputeSaliency(image);

        var h = image.Mat.Height;
        var w = image.Mat.Width;

        var cropW = Math.Min(w, size.Width);
        var cropH = Math.Min(h, size.Height);

        using var medianMap = new Mat();
        var kernelSize = Math.Max(cropW, cropH);
        // MedianBlur requires odd kernel size
        if (kernelSize % 2 == 0) kernelSize++;

        // Convert to 8-bit for MedianBlur (required by OpenCV)
        using var saliency8U = new Mat();
        saliencyMap.ConvertTo(saliency8U, DepthType.Cv8U, 255.0);

        CvInvoke.MedianBlur(saliency8U, medianMap, kernelSize);

        // Find maximum location (highest median saliency)
        double minVal = 0, maxVal = 0;
        Point minLoc = default, maxLoc = default;
        CvInvoke.MinMaxLoc(medianMap, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        // Calculate top-left corner from center point
        var topLeftX = maxLoc.X - cropW / 2;
        var topLeftY = maxLoc.Y - cropH / 2;

        // Clamp to valid range
        topLeftX = Math.Clamp(topLeftX, 0, w - cropW);
        topLeftY = Math.Clamp(topLeftY, 0, h - cropH);

        return (new Rectangle(topLeftX, topLeftY, cropW, cropH), maxVal / 255.0);
    }

    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    private static Rectangle GetCenterRoi(ImageData image, Size size)
    {
        var point = new Point
        {
            X = Math.Max(0, (image.Mat.Width - size.Width) / 2),
            Y = Math.Max(0, (image.Mat.Height - size.Height) / 2)
        };
        return new Rectangle(point, size);
    }
}