using System.Drawing;
using Emgu.CV;
using SlideGenerator.Framework.Image.Entities.FaceDetection;
using SlideGenerator.Framework.Image.Models.Roi;
using SlideGenerator.Framework.Image.Services;

namespace SlideGenerator.Framework.Image.Entities.Roi;

public sealed class RuleOfThirdsRoi : RoiCalculator
{
    public required FaceDetectorModel FaceDetectorModel { get; init; }
    public RoiOptions.RuleOfThirdsOptions Options { get; init; } = new();

    public override async ValueTask<Rectangle> CalculateRoiAsync(Mat mat, Size targetSize)
    {
        var croppedSize = new Size(
            Math.Min(mat.Width, targetSize.Width),
            Math.Min(mat.Height, targetSize.Height));
        var eyeCenter = await GetEyeCenter(mat).ConfigureAwait(false);
        return FollowRuleOfThirds(mat.Size, eyeCenter, croppedSize);
    }

    private async ValueTask<Point> GetEyeCenter(Mat mat)
    {
        var faces = await FaceDetectorModel.DetectAsync(mat).ConfigureAwait(false);
        if (faces.Count == 0)
            // fallback
            return new Point(
                (int)MathF.Round(mat.Width * Options.DefaultEyeCenterRatioX),
                (int)MathF.Round(mat.Height * Options.DefaultEyeCenterRatioY));

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

    private static Point? GetEyeCenter(Face face)
    {
        if (face is { RightEye: { } rightEye, LeftEye: { } leftEye })
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

        return ManipulatingService.ClampToBorder(
            new Rectangle(x, y, croppedSize.Width, croppedSize.Height),
            new Rectangle(default, imageSize));
    }
}