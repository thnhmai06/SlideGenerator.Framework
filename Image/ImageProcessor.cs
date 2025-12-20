using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Models;
using Emgu.CV.Saliency;
using SlideGenerator.Framework.Image.Configs;
using SlideGenerator.Framework.Image.Enums;
using SlideGenerator.Framework.Image.Exceptions;
using SlideGenerator.Framework.Image.Models;

namespace SlideGenerator.Framework.Image;

/// <summary>
///     Provides image processing operations, including ROI detection and cropping.
/// </summary>
/// <param name="roiOptions">The ROI options to use for processing.</param>
public sealed class ImageProcessor(RoiOptions roiOptions) : IDisposable
{
    // FaceDetectorYNModel.Detect is typically NOT thread-safe => serialize usage.
    private readonly SemaphoreSlim _faceDetectGate = new(1, 1);

    // Lazy init + async init of model. This only guarantees single initialization.
    private readonly Lazy<Task<FaceDetectorYNModel>> _faceModel =
        new(async () =>
        {
            var model = new FaceDetectorYNModel();
            await model.Init().ConfigureAwait(false);
            return model;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     Options controlling ROI padding and face behavior.
    /// </summary>
    public RoiOptions RoiOptions { get; } = roiOptions;

    /// <summary>
    ///     Indicates whether the face model has been successfully initialized and is ready for use.
    ///     This property is non-blocking.
    /// </summary>
    public bool IsFaceModelAvailable
    {
        get
        {
            if (!_faceModel.IsValueCreated) return false;

            var task = _faceModel.Value;
            return task is { IsCompletedSuccessfully: true, Result.Initialized: true };
        }
    }

    public void Dispose()
    {
        if (!_faceModel.IsValueCreated) return;

        var task = _faceModel.Value;
        if (task.IsCompletedSuccessfully)
            task.Result.Dispose();

        _faceDetectGate.Dispose();
    }

    /// <summary>
    ///     Get the largest size that has the same aspect ratio with the target size and fits within the original size.
    /// </summary>
    /// <param name="original">The original size.</param>
    /// <param name="target">The target size.</param>
    /// <returns>The largest size that has the same aspect ratio with the target size and fits within the original size.</returns>
    public static Size GetMaxAspectSize(Size original, Size target)
    {
        var originalAspect = original.Width / (double)original.Height;
        var targetAspect = target.Width / (double)target.Height;

        int width, height;
        if (originalAspect >= targetAspect)
        {
            height = original.Height;
            width = (int)Math.Round(height * targetAspect);
        }
        else
        {
            width = original.Width;
            height = (int)Math.Round(width / targetAspect);
        }

        width = Math.Min(width, original.Width);
        height = Math.Min(height, original.Height);
        return new Size(width, height);
    }

    /// <summary>
    ///     Crops an image to the specified region of interest.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="rect">The region of interest to crop to.</param>
    public static void Crop(ImageData image, Rectangle rect)
    {
        image.CropInPlace(rect);
    }

    /// <summary>
    ///     Resizes the specified image to the given dimensions in place.
    /// </summary>
    /// <param name="image">The image to resize (modified in place).</param>
    /// <param name="size">The size to resize to.</param>
    public static void Resize(ImageData image, Size size)
    {
        image.ResizeInPlace(size);
    }

    /// <summary>
    ///     Computes a normalized saliency map for the specified image using the spectral residual method.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <returns>Normalized saliency map (value from 0 to 1).</returns>
    public static Mat ComputeSaliency(ImageData image)
    {
        using var saliency = new StaticSaliencySpectralResidual();
        using var saliencyMap = new Mat();

        var ok = saliency.Compute(image.Mat, saliencyMap);
        if (!ok) throw new ComputeSaliencyFailed(image.FilePath);

        using var outMap = new Mat();
        saliencyMap.ConvertTo(outMap, DepthType.Cv32F);
        CvInvoke.Normalize(outMap, outMap, 0, 1, NormType.MinMax);

        return outMap.Clone();
    }

    /// <summary>
    ///     Initializes the face model if it has not already been initialized.
    /// </summary>
    /// <remarks>
    ///     This method is safe to call multiple times; initialization will only occur once.
    /// </remarks>
    /// <returns>
    ///     <see langword="true" /> if the face model was successfully initialized; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> InitFaceModelAsync()
    {
        try
        {
            var model = await _faceModel.Value.ConfigureAwait(false);
            return model.Initialized;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Crops an image using the specified cropping strategy and ROI selector.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <param name="roiSelector">
    ///     An asynchronous selector that computes the crop region (ROI)
    ///     for a given image and target size.
    /// </param>
    /// <param name="cropType">The type of cropping to perform.</param>
    /// <returns>
    ///     A task that represents the asynchronous crop operation.
    /// </returns>
    public static async ValueTask CropToRoiAsync(
        ImageData image,
        Size targetSize,
        AsyncRoiSelector roiSelector,
        CropType cropType)
    {
        switch (cropType)
        {
            case CropType.Crop:
            {
                var roi = await roiSelector(image, targetSize).ConfigureAwait(false);
                image.CropInPlace(roi);
                break;
            }
            case CropType.Fit:
            {
                var maxSize = GetMaxAspectSize(image.Size, targetSize);
                var roi = await roiSelector(image, maxSize).ConfigureAwait(false);
                image.CropInPlace(roi);
                Resize(image, targetSize);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(cropType), cropType, null);
        }
    }

    /// <summary>
    ///     Gets a ROI selector for the specified ROI type.
    /// </summary>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <returns>
    ///     A <see cref="AsyncRoiSelector" /> that computes a region of interest
    ///     for a given image and target size.
    /// </returns>
    public AsyncRoiSelector GetRoiSelector(RoiType roiType)
    {
        return roiType switch
        {
            RoiType.Center => GetCenterRoiAsync,
            RoiType.Prominent => GetProminentRoiAsync,
            RoiType.Attention => GetAttentionRoiAsync,
            _ => throw new ArgumentOutOfRangeException(nameof(roiType), roiType, null)
        };
    }

    /// <summary>
    ///     Calculates the center crop coordinates asynchronously.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    private static ValueTask<Rectangle> GetCenterRoiAsync(ImageData image, Size size)
    {
        return ValueTask.FromResult(GetCenterRoi(image, size));
    }

    /// <summary>
    ///     Finds a prominent ROI asynchronously using saliency and returns a crop rectangle within image bounds.
    /// </summary>
    /// <param name="image">The source image data used for saliency computation.</param>
    /// <param name="size">The target size that defines the base crop dimensions.</param>
    /// <returns>A Rectangle representing the most prominent region of interest within the image.</returns>
    private ValueTask<Rectangle> GetProminentRoiAsync(ImageData image, Size size)
    {
        return ValueTask.FromResult(GetProminentRoi(image, size));
    }

    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">The desired crop size.</param>
    /// <returns>A centered rectangle of the requested size (clamped to image bounds).</returns>
    private static Rectangle GetCenterRoi(ImageData image, Size size)
    {
        var point = new Point
        {
            X = Math.Max(0, (image.Mat.Width - size.Width) / 2),
            Y = Math.Max(0, (image.Mat.Height - size.Height) / 2)
        };
        return new Rectangle(point, size);
    }

    /// <summary>
    ///     Finds a prominent ROI using saliency and returns a crop rectangle within image bounds.
    /// </summary>
    /// <param name="image">The source image data used for saliency computation.</param>
    /// <param name="size">The target size that defines the base crop dimensions.</param>
    /// <returns>A Rectangle representing the most prominent region of interest within the image.</returns>
    private Rectangle GetProminentRoi(ImageData image, Size size)
    {
        using var saliencyMap = ComputeSaliency(image);

        var h = image.Mat.Height;
        var w = image.Mat.Width;

        // Base crop size (clamped to image)
        var cropW = Math.Min(w, size.Width);
        var cropH = Math.Min(h, size.Height);

        using var medianMap = new Mat();

        // Kernel size ~ crop window, clamped and forced odd >= 3
        var kernelSize = Math.Max(cropW, cropH);
        kernelSize = Math.Min(kernelSize, Math.Min(w, h));
        if (kernelSize < 3) kernelSize = 3;
        if (kernelSize % 2 == 0) kernelSize++;

        // Convert to 8U: MedianBlur needs 8U input
        using var saliency8U = new Mat();
        saliencyMap.ConvertTo(saliency8U, DepthType.Cv8U, 255.0);

        CvInvoke.MedianBlur(saliency8U, medianMap, kernelSize);

        // locate max median saliency response
        double minVal = 0, maxVal = 0;
        Point minLoc = default, maxLoc = default;
        CvInvoke.MinMaxLoc(medianMap, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        // center roi at the most salient point
        var topLeftX = Math.Clamp(maxLoc.X - cropW / 2, 0, w - cropW);
        var topLeftY = Math.Clamp(maxLoc.Y - cropH / 2, 0, h - cropH);
        var roi = new Rectangle(topLeftX, topLeftY, cropW, cropH);

        // expand roi
        roi = RoiOptions.SaliencyPaddingRatio.With(roi, new Rectangle(0, 0, w, h));
        return roi;
    }

    /// <summary>
    ///     Picks a crop window (fixed size) biased to faces, then aligned with saliency for better context.
    ///     Falls back to prominent saliency ROI when no face is available.
    /// </summary>
    /// <param name="image">The image data to analyze for attention regions. Cannot be null.</param>
    /// <param name="size">
    ///     The desired size of the ROI to extract. The ROI will be centered and constrained to fit within the
    ///     image bounds.
    /// </param>
    /// <returns>
    ///     A Rectangle representing the centered region of interest within the image, sized according to the specified
    ///     dimensions. The ROI is determined by combining saliency and face detection results, if available.
    /// </returns>
    private async ValueTask<Rectangle> GetAttentionRoiAsync(ImageData image, Size size)
    {
        var w = image.Mat.Width;
        var h = image.Mat.Height;
        var border = new Rectangle(0, 0, w, h);

        // Fixed crop window size (clamped)
        var crop = new Size(Math.Min(w, size.Width), Math.Min(h, size.Height));

        // Saliency anchor is sync under the hood; still keep async pipeline.
        var saliencyAnchor = await GetProminentRoiAsync(image, crop).ConfigureAwait(false);

        Rectangle? faceAnchor = null;
        var faces = await FaceDetectAsync(image, RoiOptions.FaceConfidence).ConfigureAwait(false);
        if (faces.Count > 0)
        {
            var faceRect = RoiOptions.FacesUnionAll ? FaceUnionAll(faces) : FacePickBest(faces);
            faceAnchor = RoiOptions.FacePaddingRatio.With(faceRect, border);
        }

        var anchor = faceAnchor is null
            ? saliencyAnchor
            : Rectangle.Union(faceAnchor.Value, saliencyAnchor);

        return CenterWindowOn(anchor, crop, border);
    }

    private static Rectangle FaceUnionAll(List<RoiCandidate> faces)
    {
        var r = faces[0].Rect;
        for (var i = 1; i < faces.Count; i++)
            r = Rectangle.Union(r, faces[i].Rect);
        return r;
    }

    private static Rectangle FacePickBest(List<RoiCandidate> faces)
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

        return best.Rect;
    }

    /// <summary>
    ///     Attempts to detect faces in the specified image.
    /// </summary>
    /// <remarks>
    ///     The method returns a list of detected face candidates. If the face model cannot be initialized
    ///     or an error occurs during detection, an empty list is returned.
    /// </remarks>
    /// <param name="image">The image data in which to search for faces. Must not be null.</param>
    /// <param name="minScore">
    ///     The minimum confidence score required for a detected face to be included in the results. Must be between 0.0 and
    ///     1.0.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a list of detected
    ///     face candidates. The list is empty if no faces are detected or if detection fails.
    /// </returns>
    private async Task<List<RoiCandidate>> FaceDetectAsync(ImageData image, float minScore)
    {
        var faces = new List<RoiCandidate>(4);

        try
        {
            // Get Face Model
            if (!await InitFaceModelAsync().ConfigureAwait(false))
                return faces;
            var model = await _faceModel.Value.ConfigureAwait(false);

            // Detect Faces
            using var raw = new Mat();
            await _faceDetectGate.WaitAsync().ConfigureAwait(false);
            try
            {
                model.Detect(image.Mat, raw);
            }
            finally
            {
                _faceDetectGate.Release();
            }

            if (raw.IsEmpty || raw.Rows <= 0 || raw.Cols < 5)
                return faces;

            var mat = raw;
            var disposeMat = false;

            try
            {
                // Ensure contiguous memory for Marshal.Copy
                if (!raw.IsContinuous)
                {
                    mat = raw.Clone();
                    disposeMat = true;
                }

                // Ensure float32 for parsing
                if (mat.Depth != DepthType.Cv32F)
                {
                    var tmp = new Mat();
                    mat.ConvertTo(tmp, DepthType.Cv32F);

                    if (disposeMat) mat.Dispose();
                    mat = tmp;
                    disposeMat = true;
                }

                var rows = mat.Rows;
                var cols = mat.Cols;
                var len = rows * cols;

                var pool = ArrayPool<float>.Shared; // ArrayPool => reduce allocations
                var buf = pool.Rent(len);

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
                        var score = buf[i + 4];

                        if (score < minScore || w <= 0 || h <= 0)
                            continue;

                        var rect = Rectangle.Intersect(new Rectangle(x, y, w, h), border);
                        if (rect.Width <= 0 || rect.Height <= 0)
                            continue;

                        faces.Add(new RoiCandidate(rect, score));
                    }
                }
                finally
                {
                    pool.Return(buf);
                }
            }
            finally
            {
                if (disposeMat) mat.Dispose();
            }
        }
        catch
        {
            faces.Clear();
        }

        return faces;
    }

    /// <summary>
    ///     Centers a fixed-size crop window on the anchor rectangle, shifting it as needed to stay within image bounds.
    /// </summary>
    /// <param name="anchor">
    ///     The rectangle on which to center the window. The window will be positioned so that its center aligns with the
    ///     center of this rectangle.
    /// </param>
    /// <param name="crop">The window size to center.</param>
    /// <param name="border">The bounding rectangle within which the window must be fully contained.</param>
    /// <returns>
    ///     A rectangle representing the position and size of the centered window, adjusted as needed to ensure it does not
    ///     extend beyond the specified border.
    /// </returns>
    private static Rectangle CenterWindowOn(Rectangle anchor, Size crop, Rectangle border)
    {
        var cx = anchor.X + anchor.Width / 2;
        var cy = anchor.Y + anchor.Height / 2;

        var x = Math.Clamp(cx - crop.Width / 2, border.Left, border.Right - crop.Width);
        var y = Math.Clamp(cy - crop.Height / 2, border.Top, border.Bottom - crop.Height);

        return new Rectangle(x, y, crop.Width, crop.Height);
    }

    /// <summary>
    ///     Represents a candidate region of interest (ROI) with an associated confidence score.
    /// </summary>
    /// <param name="Rect">The rectangular area defining the region of interest.</param>
    /// <param name="Score">
    ///     The confidence score indicating the likelihood that the region is a valid ROI. Higher values represent greater
    ///     confidence.
    /// </param>
    private readonly record struct RoiCandidate(Rectangle Rect, float Score);
}