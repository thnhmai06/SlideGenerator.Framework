namespace SlideGenerator.Framework.Image.Models.Roi;

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