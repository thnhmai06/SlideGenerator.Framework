using SlideGenerator.Framework.Image.Modules.Roi.Models;

namespace SlideGenerator.Framework.Image.Modules.Roi.Configs;

/// <summary>
///     Specifies options for configuring region-of-interest (ROI) selection in face and saliency detection scenarios.
/// </summary>
public sealed class RoiOptions
{
    /// <summary>Minimum face score (0-1) to accept a detection.</summary>
    public float FaceConfidence
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = 0.6f;

    /// <summary>If true, union all detected faces; otherwise use the best single face.</summary>
    public bool FacesUnionAll { get; set; } = true;

    /// <summary>Padding applied to the saliency-guided anchor (relative to crop window).</summary>
    public ExpandRatio SaliencyPaddingRatio { get; set; } = new(0.0f);
}