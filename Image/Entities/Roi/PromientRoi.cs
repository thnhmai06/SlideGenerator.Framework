using System.Drawing;
using OpenCvSharp;
using SlideGenerator.Framework.Image.Services;
using Size = System.Drawing.Size;

namespace SlideGenerator.Framework.Image.Entities.Roi;

/// Reviewed by @thnhmai06 at 01/03/2026 14:32:43 GMT+7
public sealed class ProminentRoi : RoiCalculator
{
    private static readonly Lazy<ProminentRoi> LazyInstance = new(() => new ProminentRoi());

    private ProminentRoi()
    {
    }

    public static ProminentRoi Instance => LazyInstance.Value;

    /// <summary>
    ///     Finds a prominent ROI using saliency and returns a crop rectangle within image bounds.
    /// </summary>
    /// <param name="image">The source image data used for saliency computation.</param>
    /// <param name="size">The target size that defines the base crop dimensions.</param>
    /// <returns>
    ///     A <see cref="Rectangle" /> representing the most prominent region of interest within the image.
    ///     If saliency could not be computed, use <see cref="CenterRoi" />.
    /// </returns>
    public static Rectangle GetProminentRoi(Mat image, Size size)
    {
        using var saliencyMap = ComputingService.ComputeSaliency(image);
        if (saliencyMap is null || saliencyMap.Empty())
            return CenterRoi.GetCenterRoi(image, size);

        var w = image.Width;
        var h = image.Height;
        var cropW = Math.Min(w, size.Width);
        var cropH = Math.Min(h, size.Height);

        var kSize = Math.Max(cropW, cropH) / 4; // scale down
        kSize = Math.Clamp(kSize, 3, 255);
        if (kSize % 2 == 0) kSize++;

        using var blurred = new Mat();
        Cv2.GaussianBlur(
            saliencyMap, blurred,
            new OpenCvSharp.Size(kSize, kSize), 0
        );

        // locate max saliency response
        Cv2.MinMaxLoc(blurred, out _, out _, out _, out var maxLoc);

        // center roi at the most salient point
        var topLeftX = Math.Clamp(maxLoc.X - cropW / 2, 0, w - cropW);
        var topLeftY = Math.Clamp(maxLoc.Y - cropH / 2, 0, h - cropH);
        var roi = new Rectangle(topLeftX, topLeftY, cropW, cropH);
        return roi;
    }

    /// <summary>
    ///     Finds a prominent ROI asynchronously using saliency and returns a crop rectangle within image bounds.
    /// </summary>
    /// <param name="mat">The source mat data used for saliency computation.</param>
    /// <param name="targetSize">The target size that defines the base crop dimensions.</param>
    /// <returns>A Rectangle representing the most prominent region of interest within the image.</returns>
    public override ValueTask<Rectangle> CalculateRoiAsync(Mat mat, Size targetSize)
    {
        return ValueTask.FromResult(GetProminentRoi(mat, targetSize));
    }
}