namespace SlideGenerator.Framework.Image.Enums;

/// <summary>
///     Specifies how an image is cropped relative to the target dimensions.
/// </summary>
public enum CropType
{
    /// <summary>
    ///     Crops the image directly.
    /// </summary>
    Crop,

    /// <summary>
    ///     Crops the largest possible region that matches the target aspect ratio,
    ///     then scales it down to the target size.
    /// </summary>
    Fit
}