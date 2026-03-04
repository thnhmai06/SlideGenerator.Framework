using System.Drawing;
using OpenCvSharp;
using SlideGenerator.Framework.Features.Image.Contracts;
using SlideGenerator.Framework.Features.Image.Models.FaceDetection;
using SlideGenerator.Framework.Features.Image.Services;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SlideGenerator.Framework.Features.Image.Entities.Roi;

/// Reviewed by @thnhmai06 at 02/03/2026 11:28:25 GMT+7
public sealed class RuleOfThirdsRoi : RoiCalculator
{
    private const float DefaultEyeCenterRatioX = 1 / 2f;
    private const float DefaultEyeCenterRatioY = 1 / 2f;

    private readonly IFaceDetectorModelProvider _faceDetectorProvider;

    /// <summary>
    ///     Initializes a new instance with face detection support.
    /// </summary>
    /// <param name="faceDetectorProvider">Face detector model provider for eye detection.</param>
    /// <exception cref="ArgumentNullException">Thrown when faceDetectorProvider is null.</exception>
    public RuleOfThirdsRoi(IFaceDetectorModelProvider faceDetectorProvider)
    {
        _faceDetectorProvider = faceDetectorProvider ?? throw new ArgumentNullException(nameof(faceDetectorProvider));
    }

    public override async ValueTask<Rectangle> CalculateRoiAsync(Mat mat, Size targetSize)
    {
        var croppedSize = new Size(
            Math.Min(mat.Width, targetSize.Width),
            Math.Min(mat.Height, targetSize.Height));
        var eyeCenter = await GetEyeCenter(mat).ConfigureAwait(false);
        var imageSize = new Size(mat.Width, mat.Height);
        return FollowRuleOfThirds(imageSize, eyeCenter, croppedSize);
    }

    private async ValueTask<Point> GetEyeCenter(Mat mat)
    {
        var model = await _faceDetectorProvider.GetCurrentModelAsync().ConfigureAwait(false);

        if (!model.IsModelAvailable)
            return GetDefaultEyeCenter(mat);

        var faces = await model.DetectAsync(mat).ConfigureAwait(false);
        if (faces.Count == 0)
            return GetDefaultEyeCenter(mat);

        var eyesCenter = new Point(0, 0);
        foreach (var eyeCenter in faces.Select(GetEyeCenter))
        {
            eyesCenter.X += eyeCenter?.X ?? 0;
            eyesCenter.Y += eyeCenter?.Y ?? 0;
        }

        eyesCenter.X /= faces.Count;
        eyesCenter.Y /= faces.Count;
        return eyesCenter;
    }

    private static Point GetDefaultEyeCenter(Mat mat)
    {
        return new Point(
            (int)MathF.Round(mat.Width * DefaultEyeCenterRatioX),
            (int)MathF.Round(mat.Height * DefaultEyeCenterRatioY));
    }

    private static Point? GetEyeCenter(FaceInfo faceInfo)
    {
        if (faceInfo is { RightEye: { } rightEye, LeftEye: { } leftEye })
            return new Point(
                (int)MathF.Round((rightEye.X + leftEye.X) / 2f),
                (int)MathF.Round((rightEye.Y + leftEye.Y) / 2f));
        return null;
    }

    private static Rectangle FollowRuleOfThirds(Size imageSize, Point eyeCenterPoint, Size croppedSize)
    {
        const float eyeLineRatioX = 1f / 2f;
        const float eyeLineRatioY = 1f / 3f;

        var x = (int)MathF.Round(eyeCenterPoint.X - croppedSize.Width * eyeLineRatioX);
        var y = (int)MathF.Round(eyeCenterPoint.Y - croppedSize.Height * eyeLineRatioY);

        return new Rectangle(x, y, croppedSize.Width, croppedSize.Height).ClampIn(new Rectangle(default, imageSize));
    }
}