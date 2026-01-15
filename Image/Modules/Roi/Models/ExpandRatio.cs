using System.Drawing;

namespace SlideGenerator.Framework.Image.Modules.Roi.Models;

/// <summary>
///     Represents expansion ratios of a rectangle.
/// </summary>
/// <remarks>
///     Use this class to specify non-uniform or uniform expansion for each side when adjusting the size of
///     a rectangle. Ratios are typically expressed as multipliers relative to the original size.
/// </remarks>
/// <param name="top">The ratio by which to expand the top side. (0 to 1)</param>
/// <param name="bottom">The ratio by which to expand the bottom side. (0 to 1)</param>
/// <param name="left">The ratio by which to expand the left side. (0 to 1)</param>
/// <param name="right">The ratio by which to expand the right side. (0 to 1)</param>
public class ExpandRatio(float top, float bottom, float left, float right)
{
    /// <summary>
    ///     Initializes an expand ratio that applies the same margin to all sides.
    /// </summary>
    /// <param name="allSides">
    ///     The margin ratio applied uniformly to the left, top, right, and bottom sides.
    /// </param>
    public ExpandRatio(float allSides)
        : this(allSides, allSides, allSides, allSides)
    {
    }

    /// <summary>
    ///     Initializes an expand ratio that applies the same vertical and horizontal margins.
    /// </summary>
    /// <param name="vertical">
    ///     The margin ratio applied to the top and bottom sides.
    /// </param>
    /// <param name="horizontal">
    ///     The margin ratio applied to the left and right sides.
    /// </param>
    public ExpandRatio(float vertical, float horizontal)
        : this(vertical, vertical, horizontal, horizontal)
    {
    }

    public float Top
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = top;

    public float Bottom
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = bottom;

    public float Left
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = left;

    public float Right
    {
        get;
        set => Math.Clamp(value, 0, 1);
    } = right;


    /// <summary>
    ///     Returns a new rectangle by applying the margin ratios to the given rectangle.
    ///     If a border is provided, the result is shifted to stay within the border
    ///     while preserving its size whenever possible.
    /// </summary>
    /// <remarks>
    ///     Margins are specified as fractions of the rectangle's width and height.
    ///     Positive values expand the rectangle outward, while negative values shrink it inward.
    ///     The input rectangle is not modified.
    /// </remarks>
    /// <param name="rect">The source rectangle.</param>
    /// <param name="border">
    ///     Optional bounding rectangle used to constrain the result
    ///     (typically the image bounds).
    /// </param>
    /// <returns>
    ///     A new rectangle adjusted by the margin ratios and constrained to the border.
    /// </returns>
    public Rectangle With(Rectangle rect, Rectangle? border = null)
    {
        var expanded = new Rectangle(
            rect.X - (int)(rect.Width * Left),
            rect.Y - (int)(rect.Height * Top),
            rect.Width + (int)(rect.Width * (Left + Right)),
            rect.Height + (int)(rect.Height * (Top + Bottom))
        );
        if (border is null) return expanded;

        var b = border.Value;

        var x = expanded.X;
        var y = expanded.Y;
        var w = expanded.Width;
        var h = expanded.Height;

        // expanded > border => shrink
        if (w > b.Width)
        {
            w = b.Width;
            x = b.X;
        }

        if (h > b.Height)
        {
            h = b.Height;
            y = b.Y;
        }

        // shift into border
        if (x < b.Left) x = b.Left;
        if (y < b.Top) y = b.Top;
        if (x + w > b.Right) x = b.Right - w;
        if (y + h > b.Bottom) y = b.Bottom - h;

        return new Rectangle(x, y, w, h);
    }
}