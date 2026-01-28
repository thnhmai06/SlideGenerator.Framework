using SlideGenerator.Framework.Image.FaceDetection;

namespace SlideGenerator.Framework.Image.Roi;

/// <summary>
///     Specifies options for configuring region-of-interest (ROI) selection in face and saliency detection scenarios.
/// </summary>
public sealed record RoiOptions
{
    public FaceDetectionOptions FaceDetection = new();
    public ProminentOptions Prominent = new();
    public RuleOfThirdsOptions RuleOfThirds = new();

    /// <summary>
    ///     Saliency detection options for ROI selection.
    /// </summary>
    public sealed record ProminentOptions
    {
        /// <summary>Padding applied to the saliency-guided anchor (relative to crop window).</summary>
        public Ratio PaddingRatio { get; init; } = new(0.0f);
    }

    /// <summary>
    ///     Rule-of-thirds options for ROI selection.
    /// </summary>
    public sealed record RuleOfThirdsOptions
    {
        /// <summary>If true, union all detected faces as center; otherwise use the best single face.</summary>
        public bool UnionAll { get; init; } = true;

        public float DefaultEyeCenterRatioX
        {
            get;
            init => Math.Clamp(value, 0, 1);
        } = 1f / 2f;

        public float DefaultEyeCenterRatioY
        {
            get;
            init => Math.Clamp(value, 0, 1);
        } = 0.35f;
    }
}