using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Models;
using SlideGenerator.Framework.Common;

namespace SlideGenerator.Framework.Image.Modules.FaceDetection.Models;

/// <summary>
///     Face Detector functionality using a YUNet-based face Detector model.
/// </summary>
/// <remarks>
///     YuNetModel manages the lifecycle and access to the underlying face detector model, ensuring that
///     detector operations are serialized for thread safety. The class should be disposed when no longer needed to release
///     resources associated with the model. Detector methods return empty results if the model cannot be initialized or if
///     detector fails. This class is not intended to be inherited.
/// </remarks>
public sealed class YuNetModel : FaceDetectorModel
{
    // FaceDetectorYNModel.Detect is typically NOT thread-safe => serialize usage.
    private readonly SemaphoreSlim _detectLock = new(1, 1);

    private readonly AsyncLazy<FaceDetectorYNModel> _model =
        new(async () =>
        {
            var model = new FaceDetectorYNModel();
            await model.Init().ConfigureAwait(false);
            return model;
        });

    public override bool IsModelAvailable
    {
        get
        {
            if (!_model.IsValueCreated) return false;
            var model = _model.GetValueOrDefault();
            return model?.Initialized ?? false;
        }
    }

    public override async Task<bool> InitAsync()
    {
        try
        {
            var model = await _model.Value.ConfigureAwait(false);
            return model.Initialized;
        }
        catch
        {
            return false;
        }
    }

    public override async Task<bool> DeInitAsync()
    {
        try
        {
            await _model.ResetAsync(model => model.Dispose()).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override async Task<List<Face>> DetectAsync(Image.Models.Image image, float minScore)
    {
        var faces = new List<Face>(4);

        if (image.Mat.IsEmpty)
            return faces;

        // Init face model on first use
        if (!await InitAsync().ConfigureAwait(false))
            return faces;

        // Run detection
        using var raw = new Mat();

        await _detectLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var model = await _model.Value.ConfigureAwait(false);
            model.Detect(image.Mat, raw);
        }
        finally
        {
            _detectLock.Release();
        }

        // expect at least 5 cols: x y w h score
        if (raw.IsEmpty || raw.Rows <= 0 || raw.Cols < 5)
            return faces;

        // Ensure contiguous (only clone when needed)
        using var contiguous = raw.IsContinuous ? null : raw.Clone();
        var src = raw.IsContinuous ? raw : contiguous!;

        // Ensure CV_32F (convert to a new Mat when needed)
        using var mat32F = src.Depth == DepthType.Cv32F ? null : new Mat();
        var mat = src.Depth == DepthType.Cv32F ? src : mat32F!;
        if (src.Depth != DepthType.Cv32F)
            src.ConvertTo(mat, DepthType.Cv32F);

        var rows = mat.Rows;
        var cols = mat.Cols;

        // expect at least 5 cols: x y w h score
        if (cols < 5 || rows <= 0)
            return faces;

        // Protect against insane sizes (can happen if model output is unexpected)
        // len = rows * cols floats
        var len64 = (long)rows * cols;
        if (len64 > int.MaxValue)
            return faces;

        var len = (int)len64;

        var usePool = len <= 1_000_000; // threshold ~ 4MB (1e6 floats ~ 4MB)
        var buf = usePool
            ? ArrayPool<float>.Shared.Rent(len)
            : new float[len]; // let GC reclaim huge arrays instead of ArrayPool hoarding them

        try
        {
            Marshal.Copy(mat.DataPointer, buf, 0, len);

            var border = new Rectangle(0, 0, image.Mat.Width, image.Mat.Height);

            for (var r = 0; r < rows; r++)
            {
                var i = r * cols;

                var x = (int)MathF.Round(buf[i + 0]);
                var y = (int)MathF.Round(buf[i + 1]);
                var w = (int)MathF.Round(buf[i + 2]);
                var h = (int)MathF.Round(buf[i + 3]);
                var score = buf[i + 14];

                if (score < minScore)
                    continue;

                var rect = Rectangle.Intersect(new Rectangle(x, y, w, h), border);
                if (rect.Width <= 0 || rect.Height <= 0)
                    continue;

                Point? rightEye = new Point((int)MathF.Round(buf[i + 4]), (int)MathF.Round(buf[i + 5]));
                Point? leftEye = new Point((int)MathF.Round(buf[i + 6]), (int)MathF.Round(buf[i + 7]));
                Point? nose = new Point((int)MathF.Round(buf[i + 8]), (int)MathF.Round(buf[i + 9]));
                Point? mouthRight = new Point((int)MathF.Round(buf[i + 10]), (int)MathF.Round(buf[i + 11]));
                Point? mouthLeft = new Point((int)MathF.Round(buf[i + 12]), (int)MathF.Round(buf[i + 13]));

                faces.Add(new Face(rect, score, rightEye, leftEye, nose, mouthRight, mouthLeft));
            }
        }
        finally
        {
            if (usePool)
                ArrayPool<float>.Shared.Return(buf);
        }

        return faces;
    }

    public override void Dispose()
    {
        if (IsModelAvailable)
        {
            var model = _model.GetValueOrDefault();
            model?.Dispose();
        }

        _model.Dispose();
        _detectLock.Dispose();
    }
}