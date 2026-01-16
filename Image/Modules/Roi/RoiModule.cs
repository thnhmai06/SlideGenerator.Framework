using System.Drawing;
using SlideGenerator.Framework.Image.Modules.Roi.Configs;
using SlideGenerator.Framework.Image.Modules.Roi.Enums;
using SlideGenerator.Framework.Image.Modules.Roi.Models;
using SlideGenerator.Framework.Image.Utilities;

namespace SlideGenerator.Framework.Image.Modules.Roi;

/// <summary>
///     Region of Interest (ROI) selection functionality.
/// </summary>
/// <remarks>
///     This class enables advanced image cropping workflows by allowing selection of prominent, centered, or
///     rule-of-thirds regions. It is designed for scenarios where automated or intelligent cropping is required, such as
///     preparing images for display or analysis. Thread safety depends on the usage of the provided options and face
///     detection engine.
/// </remarks>
/// <param name="options">
///     The options that configure ROI selection behavior, including detection parameters and selection
///     strategies.
/// </param>
public sealed partial class RoiModule(RoiOptions options)
{
    public RoiOptions Options { get; set; } = options;

    /// <summary>
    ///     Gets a ROI selector for the specified ROI type.
    /// </summary>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <returns>
    ///     A <see cref="RoiSelector" /> that computes a region of interest
    ///     for a given image and target size.
    /// </returns>
    public RoiSelector GetRoiSelector(RoiType roiType)
    {
        return roiType switch
        {
            RoiType.Center => GetCenterRoiAsync,
            RoiType.Prominent => GetProminentRoiAsync,
            RoiType.RuleOfThirds => GetRuleOfThirdsRoiAsync,
            _ => throw new ArgumentOutOfRangeException(nameof(roiType), roiType, null)
        };
    }

    /// <summary>
    ///     Crops an image using the specified cropping strategy and ROI selector.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <param name="roiSelector">
    ///     An asynchronous selector that computes the crop region (ROI)
    ///     for a given image and target size.
    /// </param>
    /// <param name="cropType">The type of cropping to perform.</param>
    /// <returns>
    ///     A task that represents the asynchronous crop operation.
    /// </returns>
    public static async ValueTask CropToRoiAsync(
        Image.Models.Image image,
        Size targetSize,
        RoiSelector roiSelector,
        CropType cropType)
    {
        switch (cropType)
        {
            case CropType.Crop:
            {
                var roi = await roiSelector(image, targetSize).ConfigureAwait(false);
                ImageUtilities.Crop(image, roi);
                break;
            }
            case CropType.Fit:
            {
                var maxSize = ImageUtilities.GetMaxAspectSize(image.Size, targetSize);
                var roi = await roiSelector(image, maxSize).ConfigureAwait(false);
                ImageUtilities.Crop(image, roi);
                ImageUtilities.Resize(image, targetSize);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(cropType), cropType, null);
        }
    }
}