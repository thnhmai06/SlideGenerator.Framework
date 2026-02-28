using System.Drawing;
using Emgu.CV;
using SlideGenerator.Framework.Image.Models.Roi;

namespace SlideGenerator.Framework.Image.Entities.Roi;

/// Reviewed by @thnhmai06 at 01/03/2026 02:07:08 GMT+7
public abstract class RoiCalculator
{
    /// <summary>
    ///     Asynchronously calculates the region of interest (ROI) within the specified image that best matches the given
    ///     target size.
    /// </summary>
    /// <param name="mat">The source mat in which to search for the region of interest. Cannot be null.</param>
    /// <param name="targetSize">The desired size of the region of interest to locate within the image, in pixels.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a Rectangle specifying the
    ///     coordinates and size of the detected region of interest.
    /// </returns>
    public abstract ValueTask<Rectangle> CalculateRoiAsync(Mat mat, Size targetSize);
}

public static class RoiCalculatorExtensions
{
    public static RoiCalculator GetCalculator(this RoiType type)
    {
        return type switch
        {
            RoiType.Center => CenterRoi.Instance,
            RoiType.Prominent => ProminentRoi.Instance,
            RoiType.RuleOfThirds => RuleOfThirdsRoi.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}