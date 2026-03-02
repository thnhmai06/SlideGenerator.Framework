using System.Drawing;
using OpenCvSharp;
using SlideGenerator.Framework.Features.Image.Contracts;
using SlideGenerator.Framework.Features.Image.Models.Roi;
using Size = System.Drawing.Size;

namespace SlideGenerator.Framework.Features.Image.Entities.Roi;

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
    /// <summary>
    ///     Gets ROI calculator for the specified type.
    ///     CenterRoi and ProminentRoi are singletons.
    ///     RuleOfThirds requires face detector provider and creates a new instance.
    /// </summary>
    /// <param name="type">The type of ROI calculation to perform.</param>
    /// <param name="faceDetectorProvider">Face detector model provider. Required for RuleOfThirds.</param>
    /// <returns>ROI calculator instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when RuleOfThirds is requested without face detector provider.</exception>
    public static Task<RoiCalculator> GetCalculator(this RoiType type,
        IFaceDetectorModelProvider? faceDetectorProvider = null)
    {
        RoiCalculator calculator = type switch
        {
            RoiType.Center => CenterRoi.Instance,
            RoiType.Prominent => ProminentRoi.Instance,
            RoiType.RuleOfThirds when faceDetectorProvider != null =>
                new RuleOfThirdsRoi(faceDetectorProvider),
            RoiType.RuleOfThirds =>
                throw new ArgumentNullException(nameof(faceDetectorProvider),
                    "Face detector model provider is required for RuleOfThirds ROI calculation."),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return Task.FromResult(calculator);
    }
}