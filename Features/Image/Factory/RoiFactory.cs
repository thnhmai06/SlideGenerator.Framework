using SlideGenerator.Framework.Image.Entities.FaceDetection;
using SlideGenerator.Framework.Image.Entities.Roi;
using SlideGenerator.Framework.Image.Models.Roi;

namespace SlideGenerator.Framework.Image.Factory;

/// <summary>
///     Region of Interest (ROI) selection functionality.
/// </summary>
/// <remarks>
///     This class enables advanced image cropping workflows by allowing selection of prominent, centered, or
///     rule-of-thirds regions. It is designed for scenarios where automated or intelligent cropping is required, such as
///     preparing images for display or analysis. Thread safety depends on the usage of the provided options and face
///     detection engine.
/// </remarks>
public sealed class RoiFactory
{
    public required RoiOptions Options { get; init; }
    public FaceDetectorModel? FaceDetector { get; init; }

    /// <summary>
    ///     Gets a ROI selector for the specified ROI type.
    /// </summary>
    /// <param name="type">The type of ROI detection to use.</param>
    /// <returns>
    ///     A <see cref="RoiCalculator" /> that computes a region of interest
    ///     for a given mat and target size.
    /// </returns>
    public RoiCalculator GetCalculator(RoiType type)
    {
        return type switch
        {
            RoiType.Center => new CenterRoi(),

            RoiType.Prominent => new ProminentRoi { Options = Options.Prominent },

            RoiType.RuleOfThirds => new RuleOfThirdsRoi
            {
                Options = Options.RuleOfThirds,
                FaceDetectorModel =
                    FaceDetector ?? throw new InvalidOperationException("FaceDetector model is not set.")
            },

            _ => throw new NotImplementedException()
        };
    }
}