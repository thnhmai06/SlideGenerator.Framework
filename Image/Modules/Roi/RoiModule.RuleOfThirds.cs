using System.Drawing;
using SlideGenerator.Framework.Image.Modules.FaceDetection;
using SlideGenerator.Framework.Image.Modules.FaceDetection.Models;
using SlideGenerator.Framework.Image.Utilities;

namespace SlideGenerator.Framework.Image.Modules.Roi;

public sealed partial class RoiModule
{
    private const float DefaultEyeLineRatio = 0.35f;
    private const float RuleOfThirdsEyeLineRatio = 1f / 3f;

    public required FaceDetectorModel? FaceDetectorModel { get; init; }

    /// <summary>
    ///     Picks a crop window (fixed size) using a rule-of-thirds anchor from detected eye landmarks.
    /// </summary>
    /// <remarks>
    ///     If no face/landmark is available, the anchor falls back to a default eye line.
    /// </remarks>
    /// <param name="image">The image data to analyze for rule-of-thirds regions. Cannot be null.</param>
    /// <param name="size">
    ///     The desired size of the ROI to extract. The ROI will be positioned to keep the eye line near the top third.
    /// </param>
    /// <returns>
    ///     A Rectangle representing the rule-of-thirds region of interest within the image, sized according to the
    ///     specified dimensions.
    /// </returns>
    private async ValueTask<Rectangle> GetRuleOfThirdsRoiAsync(Image.Models.Image image, Size size)
    {
        var w = image.Mat.Width;
        var h = image.Mat.Height;
        var border = new Rectangle(0, 0, w, h);

        var crop = new Size(Math.Min(w, size.Width), Math.Min(h, size.Height));
        var eyeCenter = await GetEyeCenterAsync(image, border).ConfigureAwait(false);
        return FollowRuleOfThirds(border, eyeCenter, crop);
    }

    /// <summary>
    /// Determines the center point between the eyes within the detected face region of the specified
    /// image.
    /// </summary>
    /// <remarks>If no face is detected in the image, the method returns a point horizontally centered within
    /// the border and vertically positioned at a standard eye line ratio. The returned point is always constrained to
    /// the provided border.</remarks>
    /// <param name="image">The image in which to detect the face and calculate the eye center.</param>
    /// <param name="border">The bounding rectangle that defines the area within which the eye center point will be clamped.</param>
    /// <returns>A <see cref="Point"/> representing the center between the eyes, clamped to the specified border. If no face is
    /// detected, returns a default point along the typical eye line.</returns>
    private async ValueTask<Point> GetEyeCenterAsync(Image.Models.Image image, Rectangle border)
    {
        var faces = await FaceDetectorModel!.DetectAsync(image, Options.FaceConfidence).ConfigureAwait(false);
        if (faces.Count > 0 &&
            FaceProcessingHelpers.TryGetEyeCentroid(
                faces,
                Options.FacesUnionAll,
                DefaultEyeLineRatio,
                out var eyeCenter))
            return ImageUtilities.ClampToBorder(eyeCenter, border);
        return new Point(border.Width / 2, (int)MathF.Round(border.Height * DefaultEyeLineRatio));
    }

    /// <summary>
    /// Calculates a crop rectangle positioned according to the rule of thirds, centering the specified eye point
    /// horizontally and aligning it vertically based on a standard eye-line ratio.
    /// </summary>
    /// <remarks>The returned rectangle is clamped to ensure it does not exceed the boundaries of the provided
    /// border. This method is typically used in image processing scenarios to generate visually balanced crops centered
    /// around a subject's eyes.</remarks>
    /// <param name="border">The bounding rectangle that defines the limits within which the crop rectangle must remain.</param>
    /// <param name="eyeCenter">The point representing the center of the eye, used as a reference for positioning the crop rectangle.</param>
    /// <param name="crop">The size of the desired crop rectangle.</param>
    /// <returns>A rectangle representing the crop area, adjusted to fit within the specified border and positioned according to
    /// the rule of thirds relative to the eye center.</returns>
    private static Rectangle FollowRuleOfThirds(Rectangle border, Point eyeCenter, Size crop)
    {
        var x = (int)MathF.Round(eyeCenter.X - crop.Width / 2f);
        var y = (int)MathF.Round(eyeCenter.Y - crop.Height * RuleOfThirdsEyeLineRatio);

        return ImageUtilities.ClampToBorder(new Rectangle(x, y, crop.Width, crop.Height), border);
    }
}