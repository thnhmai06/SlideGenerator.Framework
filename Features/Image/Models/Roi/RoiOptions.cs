namespace SlideGenerator.Framework.Image.Models.Roi;

/// <summary>
///     Specifies options for configuring region-of-interest (ROI) selection in face and saliency detection scenarios.
/// </summary>
public sealed record RoiOptions
{
    public readonly ProminentOptions Prominent = new();
    public readonly RuleOfThirdsOptions RuleOfThirds = new();
}