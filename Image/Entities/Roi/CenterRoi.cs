using System.Drawing;
using OpenCvSharp;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SlideGenerator.Framework.Image.Entities.Roi;

/// Reviewed by @thnhmai06 at 01/03/2026 02:09:00 GMT+7
public sealed class CenterRoi : RoiCalculator
{
    private static readonly Lazy<CenterRoi> LazyInstance = new(() => new CenterRoi());

    private CenterRoi()
    {
    }

    public static CenterRoi Instance => LazyInstance.Value;

    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    /// <param name="mat">The source mat.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    public static Rectangle GetCenterRoi(Mat mat, Size size)
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