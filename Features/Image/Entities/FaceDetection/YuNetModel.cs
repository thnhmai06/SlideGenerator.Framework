using System.Buffers;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Models;

namespace SlideGenerator.Framework.Image.Entities.FaceDetection;

/// <summary>
///     Asynchronous wrapper for <see cref="FaceDetectorYNModel" />.
/// </summary>
public sealed class YuNetModel : FaceDetectorModel
{
    private readonly AsyncLazy<FaceDetectorYNModel> _model =
        new(async () =>
        {
            var model = new FaceDetectorYNModel();
            await model.Init().ConfigureAwait(false);
            return model;
        });

    /// <summary>
    ///     Gets the semaphore used to coordinate access to lock detection operations.
    /// </summary>
    /// <remarks>
    ///     Use this semaphore to ensure that lock detection logic is executed in a thread-safe manner.
    ///     The semaphore is initialized with a single slot, allowing only one concurrent operation. This property is
    ///     immutable and set during object initialization.
    /// </remarks>
    public SemaphoreSlim DetectLock { private get; init; } = new(1, 1);

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

    public override async Task<List<Face>> DetectAsync(Mat mat)
    {
        var faces = new List<Face>();

        if (mat.IsEmpty) return faces;

        // Init face model on first use
        if (!await InitAsync().ConfigureAwait(false)) return faces;

        using var raw = new Mat();
        await DetectLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var model = await _model.Value.ConfigureAwait(false);
            model.Detect(mat, raw);
        }
        finally
        {
            DetectLock.Release();
        }

        if (raw.IsEmpty || raw.Rows == 0 || raw.Cols < 15)
            return faces;

        var rows = raw.Rows;
        var cols = raw.Cols;
        var len = rows * cols;

        // Use ArrayPool to reduce GC pressure when too many
        var usePool = len <= 1_000_000;
        var buf = usePool ? ArrayPool<float>.Shared.Rent(len) : new float[len];

        try
        {
            raw.CopyTo(buf);

            var border = new Rectangle(0, 0, mat.Width, mat.Height);

            for (var r = 0; r < rows; r++)
            {
                var i = r * cols;
                var score = buf[i + 14];
                if (score < Options.Confidence) continue;

                var x = (int)MathF.Round(buf[i + 0]);
                var y = (int)MathF.Round(buf[i + 1]);
                var w = (int)MathF.Round(buf[i + 2]);
                var h = (int)MathF.Round(buf[i + 3]);

                var rect = Rectangle.Intersect(new Rectangle(x, y, w, h), border);
                if (rect.Width <= 0 || rect.Height <= 0) continue;

                // Only parse landmarks if the face is valid
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
            if (usePool) ArrayPool<float>.Shared.Return(buf);
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
        DetectLock.Dispose();
    }
}