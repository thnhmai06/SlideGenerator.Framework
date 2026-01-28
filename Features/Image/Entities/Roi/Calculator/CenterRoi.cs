using System.Drawing;
using Emgu.CV;

namespace SlideGenerator.Framework.Image.Roi;

public sealed class CenterRoi : RoiCalculator
{
    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    /// <param name="mat">The source mat.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    private static Rectangle GetCenterRoi(Mat mat, Size size)
    {
        var point = new Point
        {
            X = Math.Max(0, (mat.Width - size.Width) / 2),
            Y = Math.Max(0, (mat.Height - size.Height) / 2)
        };
        return new Rectangle(point, size);
    }

    /// <summary>
    ///     Calculates the center crop coordinates asynchronously.
    /// </summary>
    /// <param name="mat">The source mat.</param>
    /// <param name="targetSize">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    public override ValueTask<Rectangle> CalculateRoiAsync(Mat mat, Size targetSize)
    {
        return ValueTask.FromResult(GetCenterRoi(mat, targetSize));
    }
}