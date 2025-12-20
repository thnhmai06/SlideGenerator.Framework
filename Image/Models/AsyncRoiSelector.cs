using System.Drawing;

namespace SlideGenerator.Framework.Image.Models;

/// <summary>
///     Asynchronously computes a region of interest (ROI) for an image and a target size.
/// </summary>
/// <param name="image">The source image.</param>
/// <param name="targetSize">The desired ROI size.</param>
/// <returns>
///     A task that represents the asynchronous operation. The task result is a
///     <see cref="Rectangle" /> representing the computed region of interest,
///     constrained to the image bounds.
/// </returns>
public delegate ValueTask<Rectangle> AsyncRoiSelector(
    ImageData image,
    Size targetSize);