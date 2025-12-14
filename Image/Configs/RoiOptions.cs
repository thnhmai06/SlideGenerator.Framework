using SlideGenerator.Framework.Image.Models;

namespace SlideGenerator.Framework.Image.Configs;

/// <summary>
///     Specifies options for configuring region-of-interest (ROI) selection in face and saliency detection scenarios.
/// </summary>
/// <remarks>
///     Use this class to adjust how faces and salient regions are detected and how padding is applied to the
///     resulting ROI. These options influence which faces are considered, how multiple faces are handled, and how much
///     additional area is included around detected regions. All properties are settable, allowing customization for
///     different detection requirements.
/// </remarks>
public sealed class RoiOptions
{
    /// <summary>Minimum face score (0–1) to accept a detection.</summary>
    public float FaceConfidence
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = 0.6f;

    /// <summary>Padding applied to the chosen face rectangle (relative to face size).</summary>
    public ExpandRatio FacePaddingRatio { get; set; } = new(0.15f);

    /// <summary>If true, union all detected faces; otherwise use the best single face.</summary>
    public bool FacesUnionAll { get; set; } = true;

    /// <summary>Padding applied to the saliency-guided anchor (relative to crop window).</summary>
    public ExpandRatio SaliencyPaddingRatio { get; set; } = new(0.0f);
}