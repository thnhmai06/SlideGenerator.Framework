namespace SlideGenerator.Framework.Image.Models.Roi;

/// <summary>
///     Saliency detection options for ROI selection.
/// </summary>
public sealed record ProminentOptions
{
    /// <summary>Padding applied to the saliency-guided anchor (relative to crop window).</summary>
    public Ratio PaddingRatio { get; init; } = new(0.0f);
}