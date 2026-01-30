using System.Drawing;
using Emgu.CV;
using SlideGenerator.Framework.Image.Models.Roi;
using SlideGenerator.Framework.Image.Services;

namespace SlideGenerator.Framework.Image.Entities.Roi;

public sealed class ProminentRoi : RoiCalculator
{
    public RoiOptions.ProminentOptions Options { get; init; } = new();

    /// <summary>
    ///     Finds a prominent ROI using saliency and returns a crop rectangle within image bounds.
    /// </summary>
    /// <param name="image">The source image data used for saliency computation.</param>
    /// <param name="size">The target size that defines the base crop dimensions.</param>
    /// <returns>A Rectangle representing the most prominent region of interest within the image.</returns>
    private Rectangle GetProminentRoi(Mat image, Size size)
    {
        using var saliencyMap = ComputingService.ComputeSaliency(image);

        var h = image.Height;
        var w = image.Width;

        // Base crop size (clamped to image)
        var cropW = Math.Min(w, size.Width);
        var cropH = Math.Min(h, size.Height);

        var kSize = Math.Max(cropW, cropH) / 4; // scale down
        kSize = Math.Clamp(kSize, 3, 255);
        if (kSize % 2 == 0) kSize++;

        using var blurred = new Mat();
        CvInvoke.GaussianBlur(
            saliencyMap, blurred,
            new Size(kSize, kSize), 0
        );

        // locate max saliency response
        double minVal = 0, maxVal = 0;
        Point minLoc = default, maxLoc = default;
        CvInvoke.MinMaxLoc(blurred, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        // center roi at the most salient point
        var topLeftX = Math.Clamp(maxLoc.X - cropW / 2, 0, w - cropW);
        var topLeftY = Math.Clamp(maxLoc.Y - cropH / 2, 0, h - cropH);
        var roi = new Rectangle(topLeftX, topLeftY, cropW, cropH);

        // expand roi
        roi = Options.PaddingRatio.Expand(roi, new Rectangle(0, 0, w, h));
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