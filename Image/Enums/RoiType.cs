namespace SlideGenerator.Framework.Image.Enums;

/// <summary>
///     Specifies the region of interest (ROI) detection type for image cropping.
/// </summary>
public enum RoiType
{
    /// <summary>
    ///     Prominent + Using Face Detection to find the most important region of the image.
    /// </summary>
    Attention,

    /// <summary>
    ///     Detects the most prominent/salient region of the image using spectral residual saliency.
    /// </summary>
    Prominent,

    /// <summary>
    ///     Uses the center region of the image.
    /// </summary>
    Center
}