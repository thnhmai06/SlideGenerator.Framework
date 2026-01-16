using System.Drawing;
using SlideGenerator.Framework.Image.Modules.FaceDetection.Models;

namespace SlideGenerator.Framework.Image.Modules.FaceDetection;

/// <summary>
///     Provides helper methods for processing facial detection results, including calculating eye centroids, selecting the
///     best face, and determining anchor points for facial features.
/// </summary>
/// <remarks>
///     This class is intended for internal use in facial analysis workflows and is not thread-safe. All
///     methods are static and operate on collections of detected face data. The methods assume valid input and do not
///     perform argument validation; callers should ensure that input collections are not empty and that parameters are
///     within expected ranges.
/// </remarks>
internal static class FaceProcessingHelpers
{
    /// <summary>
    ///     Attempts to calculate the centroid point between the eyes from a collection of detected faces.
    /// </summary>
    /// <remarks>
    ///     If no faces are provided, the method returns <see langword="false" /> and sets
    ///     <paramref
    ///         name="center" />
    ///     to the default value. When eye center data is unavailable, the method uses a fallback anchor
    ///     point based on the provided ratio.
    /// </remarks>
    /// <param name="faces">A read-only list of detected faces to analyze for eye centroid calculation. Must not be empty.</param>
    /// <param name="useAllFaces">
    ///     Indicates whether to use all faces in the collection for averaging the eye centroid. If <see langword="false" />,
    ///     only the best face is used.
    /// </param>
    /// <param name="eyeLineRatio">
    ///     The ratio used to determine the fallback anchor point along the eye line if eye center data is unavailable. Must
    ///     be a value between 0.0 and 1.0.
    /// </param>
    /// <param name="center">
    ///     When this method returns, contains the calculated centroid point between the eyes if successful; otherwise,
    ///     contains a fallback anchor point.
    /// </param>
    /// <returns><see langword="true" /> if the eye centroid was successfully calculated; otherwise, <see langword="false" />.</returns>
    internal static bool TryGetEyeCentroid(
        IReadOnlyList<Face> faces,
        bool useAllFaces,
        float eyeLineRatio,
        out Point center)
    {
        if (faces.Count == 0)
        {
            center = default;
            return false;
        }

        if (!useAllFaces)
        {
            var best = PickBestFace(faces);
            if (best.TryGetEyeCenter(out var bestEye))
            {
                center = Point.Round(bestEye);
                return true;
            }

            center = GetEyeLineAnchorPoint(best.Rect, eyeLineRatio);
            return true;
        }

        if (TryGetMeanEyeCenter(faces, out var avg))
        {
            center = Point.Round(avg);
            return true;
        }

        center = GetEyeLineAnchorPoint(UnionFaces(faces), eyeLineRatio);
        return true;
    }

    /// <summary>
    ///     Calculates the smallest rectangle that fully contains all face rectangles in the specified collection.
    /// </summary>
    /// <remarks>
    ///     If the <paramref name="faces" /> collection contains only one element, its rectangle is
    ///     returned. The method does not validate that the rectangles are non-empty or non-overlapping.
    /// </remarks>
    /// <param name="faces">
    ///     A read-only list of <see cref="Face" /> objects whose rectangles will be combined into a single bounding
    ///     rectangle. Must contain at least one element.
    /// </param>
    /// <returns>A <see cref="Rectangle" /> representing the union of all face rectangles in the collection.</returns>
    private static Rectangle UnionFaces(IReadOnlyList<Face> faces)
    {
        var r = faces[0].Rect;
        for (var i = 1; i < faces.Count; i++)
            r = Rectangle.Union(r, faces[i].Rect);
        return r;
    }

    /// <summary>
    ///     Selects the face with the highest score from the provided collection. If multiple faces have the same score, the
    ///     face with the largest area is chosen.
    /// </summary>
    /// <remarks>
    ///     This method assumes that the input list contains at least one face. No validation is
    ///     performed on the input; passing an empty list will result in an exception.
    /// </remarks>
    /// <param name="faces">A read-only list of faces to evaluate. Must contain at least one element.</param>
    /// <returns>The face with the highest score, or the largest area if scores are equal.</returns>
    private static Face PickBestFace(IReadOnlyList<Face> faces)
    {
        var best = faces[0];
        var bestArea = best.Rect.Width * best.Rect.Height;

        for (var i = 1; i < faces.Count; i++)
        {
            var f = faces[i];
            var area = f.Rect.Width * f.Rect.Height;

            if (f.Score > best.Score || (Math.Abs(f.Score - best.Score) < 1e-6f && area > bestArea))
            {
                best = f;
                bestArea = area;
            }
        }

        return best;
    }

    /// <summary>
    ///     Attempts to compute the mean center point of all detected eyes from the provided faces.
    /// </summary>
    /// <remarks>
    ///     Faces without a valid eye center are ignored in the calculation. If no faces contain a valid
    ///     eye center, the method returns false and sets the output parameter to its default value.
    /// </remarks>
    /// <param name="faces">
    ///     A read-only list of face objects to analyze for eye center positions. Each face is evaluated for a valid eye
    ///     center.
    /// </param>
    /// <param name="center">
    ///     When this method returns, contains the mean center point of all eyes found if successful; otherwise, the default
    ///     value.
    /// </param>
    /// <returns>true if at least one eye center was found and the mean was computed; otherwise, false.</returns>
    private static bool TryGetMeanEyeCenter(IReadOnlyList<Face> faces, out PointF center)
    {
        float totalX = 0;
        float totalY = 0;
        var count = 0;

        foreach (var face in faces)
        {
            if (!face.TryGetEyeCenter(out var eyeCenter))
                continue;

            totalX += eyeCenter.X;
            totalY += eyeCenter.Y;
            count++;
        }

        if (count == 0)
        {
            center = default;
            return false;
        }

        center = new PointF(totalX / count, totalY / count);
        return true;
    }

    /// <summary>
    ///     Calculates the anchor point on the vertical eye line within a detected face rectangle.
    /// </summary>
    /// <remarks>
    ///     This method is useful for facial feature alignment tasks, such as overlaying graphics or
    ///     performing further analysis based on eye position. The anchor point is calculated using integer rounding for the
    ///     vertical position.
    /// </remarks>
    /// <param name="face">
    ///     The rectangle representing the detected face area. The anchor point will be calculated relative to
    ///     this region.
    /// </param>
    /// <param name="eyeLineRatio">
    ///     The relative vertical position of the eye line within the face rectangle, expressed as a ratio between 0.0 (top)
    ///     and 1.0 (bottom). Must be between 0.0 and 1.0.
    /// </param>
    /// <returns>
    ///     A Point representing the anchor position on the eye line, horizontally centered within the face rectangle and
    ///     vertically positioned according to the specified ratio.
    /// </returns>
    private static Point GetEyeLineAnchorPoint(Rectangle face, float eyeLineRatio)
    {
        return new Point(
            face.X + face.Width / 2,
            face.Y + (int)MathF.Round(face.Height * eyeLineRatio));
    }
}