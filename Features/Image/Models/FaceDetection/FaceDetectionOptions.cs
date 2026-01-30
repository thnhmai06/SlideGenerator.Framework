namespace SlideGenerator.Framework.Image.Models.FaceDetection;

/// <summary>
///     Face detection options for ROI selection.
/// </summary>
public sealed record FaceDetectionOptions
{
    /// <summary>Minimum face score (0-1) to accept a detection.</summary>
    public float Confidence
    {
        get;
        init => Math.Clamp(value, 0, 1);
    } = 0.6f;
}