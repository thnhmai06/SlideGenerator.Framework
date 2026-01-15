using System.Drawing;

namespace SlideGenerator.Framework.Image.Modules.Roi;

public sealed partial class RoiModule
{
    /// <summary>
    ///     Calculates the center crop coordinates asynchronously.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    private static ValueTask<Rectangle> GetCenterRoiAsync(Image.Models.Image image, Size size)
    {
        return ValueTask.FromResult(GetCenterRoi(image, size));
    }

    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    private static Rectangle GetCenterRoi(Image.Models.Image image, Size size)
    {
        var point = new Point
        {
            X = Math.Max(0, (image.Mat.Width - size.Width) / 2),
            Y = Math.Max(0, (image.Mat.Height - size.Height) / 2)
        };
        return new Rectangle(point, size);
    }
}