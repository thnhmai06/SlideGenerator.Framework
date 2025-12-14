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
/// <remarks>Make sure to call <see cref="InitFaceModelAsync" /> before using face detection features.</remarks>
public sealed class ImageProcessor(RoiOptions roiOptions) : IDisposable
{
    private readonly SemaphoreSlim _faceInitGate = new(1, 1);

    private readonly Lock _faceLock = new();
    private readonly FaceDetectorYNModel _faceModel = new();

    /// <summary>
    ///     Is the Face model ready for use?
    /// </summary>
    public bool IsFaceAvailable => _faceModel.Initialized;

    public RoiOptions RoiOptions { get; } = roiOptions;

    public void Dispose()
    {
        _faceModel.Dispose();
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
    /// <returns>Normalized saliency map (value from 0 to 1).</returns>
    public static Mat ComputeSaliency(ImageData image)
    {
        using var saliency = new StaticSaliencySpectralResidual();
        using var saliencyMap = new Mat();

        var ok = saliency.Compute(image.Mat, saliencyMap);
        if (!ok) throw new SaliencyComputationException(image.FilePath);

        using var outMap = new Mat();
        saliencyMap.ConvertTo(outMap, DepthType.Cv32F);
        CvInvoke.Normalize(outMap, outMap, 0, 1, NormType.MinMax);

        return outMap.Clone();
    }

    /// <summary>
    ///     Initializes the face model if it has not already been initialized.
    /// </summary>
    /// <remarks>
    ///     This method is safe to call multiple times; initialization will only occur if the face model
    ///     is not already initialized.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the face model
    ///     is initialized; otherwise, <see langword="false" />.
    /// </returns>
    public async Task<bool> InitFaceModelAsync()
    {
        if (IsFaceAvailable) return true;

        await _faceInitGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!IsFaceAvailable)
                await _faceModel.Init().ConfigureAwait(false);
            return IsFaceAvailable;
        }
        finally
        {
            _faceInitGate.Release();
        }
    }

    /// <summary>
    ///     Crops an image to the optimal region based on the specified ROI type.
    /// </summary>
    /// <param name="image">The image to crop (modified in place).</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <param name="cropType">The type of cropping to perform.</param>
    public void CropToRoi(ImageData image, Size targetSize, RoiType roiType, CropType cropType)
    {
        switch (cropType)
        {
            case CropType.Crop:
            {
                var roi = GetRoi(image, targetSize, roiType);
                image.CropInPlace(roi);
                break;
            }
            case CropType.Fit:
            {
                var maxSize = GetMaxAspectSize(image.Size, targetSize);
                var roi = GetRoi(image, maxSize, roiType);
                image.CropInPlace(roi);
                Resize(image, targetSize);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(cropType), cropType, null);
        }
    }

    /// <summary>
    ///     Gets the region of interest (ROI) for cropping an image to a target size.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="targetSize">The target size for cropping.</param>
    /// <param name="roiType">The type of ROI detection to use.</param>
    /// <returns>A rectangle representing the optimal crop region.</returns>
    public Rectangle GetRoi(ImageData image, Size targetSize, RoiType roiType)
    {
        return roiType switch
        {
            RoiType.Prominent => GetProminentRoi(image, targetSize),
            RoiType.Center => GetCenterRoi(image, targetSize),
            RoiType.Attention => GetAttentionRoi(image, targetSize),
            _ => throw new ArgumentOutOfRangeException(nameof(roiType), roiType, null)
        };
    }

    /// <summary>
    ///     Calculates the center crop coordinates.
    /// </summary>
    public static Rectangle GetCenterRoi(ImageData image, Size size)
    {
        var point = new Point
        {
            X = Math.Max(0, (image.Mat.Width - size.Width) / 2),
            Y = Math.Max(0, (image.Mat.Height - size.Height) / 2)
        };
        return new Rectangle(point, size);
    }

    /// <summary>Finds a prominent ROI using saliency and returns a crop rectangle within image bounds.</summary>
    /// <remarks>
    ///     The method computes a saliency map and uses the median saliency response
    ///     to locate the most visually prominent region. The resulting ROI is centered
    ///     at the peak saliency location and sized according to the requested target size.
    ///     <para />
    /// </remarks>
    /// <param name="image">
    ///     The source image data used for saliency computation.
    /// </param>
    /// <param name="size">
    ///     The target size that defines the base crop dimensions.
    /// </param>
    /// <returns>
    ///     A Rectangle representing the most prominent region of interest within the image,
    /// </returns>
    public Rectangle GetProminentRoi(ImageData image, Size size)
    {
        using var saliencyMap = ComputeSaliency(image);

        var h = image.Mat.Height;
        var w = image.Mat.Width;

        // Base crop size (clamped to image)
        var cropW = Math.Min(w, size.Width);
        var cropH = Math.Min(h, size.Height);

        using var medianMap = new Mat();

        var kernelSize = Math.Max(cropW, cropH);
        kernelSize = Math.Min(kernelSize, Math.Min(w, h));
        if (kernelSize < 3) kernelSize = 3;
        if (kernelSize % 2 == 0) kernelSize++;

        // Convert to 8U
        using var saliency8U = new Mat(); // MedianBlur needs 8U input
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
        roi = RoiOptions.SaliencyPaddingRatio.With(
            roi,
            new Rectangle(0, 0, w, h)
        );

        return roi;
    }

    /// <summary>
    ///     Picks a crop window (fixed size) biased to faces, then aligned with saliency for better context.
    ///     Falls back to prominent saliency ROI when no face is available.
    /// </summary>
    /// <remarks>
    ///     If one or more faces are detected in the image, the ROI will encompass both the most salient
    ///     region and the detected faces. Otherwise, the ROI is based solely on image saliency. The returned ROI will not
    ///     exceed the image boundaries.
    /// </remarks>
    /// <param name="image">The image data to analyze for attention regions. Cannot be null.</param>
    /// <param name="size">
    ///     The desired size of the ROI to extract. The ROI will be centered and constrained to fit within the
    ///     image bounds.
    /// </param>
    /// <returns>
    ///     A Rectangle representing the centered region of interest within the image, sized according to the specified
    ///     dimensions. The ROI is determined by combining saliency and face detection results, if available.
    /// </returns>
    public Rectangle GetAttentionRoi(ImageData image, Size size)
    {
        var w = image.Mat.Width;
        var h = image.Mat.Height;
        var border = new Rectangle(0, 0, w, h);

        var crop = new Size(Math.Min(w, size.Width), Math.Min(h, size.Height));
        var saliencyAnchor = GetProminentRoi(image, crop);

        Rectangle? faceAnchor = null;
        if (FaceTryDetect(image, RoiOptions.FaceConfidence, out var faces) && faces.Count > 0)
        {
            var faceRect = RoiOptions.FacesUnionAll ? FaceUnionAll(faces) : FacePickBest(faces);
            faceAnchor = RoiOptions.FacePaddingRatio.With(faceRect, border);
        }

        var anchor = faceAnchor is null
            ? saliencyAnchor
            : Rectangle.Union(faceAnchor.Value, saliencyAnchor);

        return CenterWindowOn(anchor, crop, border);
    }

    /// <summary>
    ///     Calculates the smallest rectangle that contains all face regions in the specified list.
    /// </summary>
    /// <param name="faces">
    ///     A list of face region candidates. Each candidate must have a valid rectangle. The list must contain at least one
    ///     element.
    /// </param>
    /// <returns>A Rectangle that represents the union of all face regions in the list.</returns>
    private static Rectangle FaceUnionAll(List<RoiCandidate> faces)
    {
        var r = faces[0].Rect;
        for (var i = 1; i < faces.Count; i++)
            r = Rectangle.Union(r, faces[i].Rect);
        return r;
    }

    /// <summary>
    ///     Selects the best face candidate from a list based on score and area.
    /// </summary>
    /// <param name="faces">A list of face region candidates to evaluate. Must contain at least one element.</param>
    /// <returns>
    ///     A Rectangle representing the region of the best face candidate, determined by the highest score. If multiple
    ///     candidates have the same score, the one with the largest area is selected.
    /// </returns>
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
    ///     Attempts to detect faces in the specified image and outputs a list of detected face candidates.
    /// </summary>
    /// <remarks>
    ///     If face detection is disabled or the detection model is unavailable, the method returns false
    ///     and no detection is performed. If an error occurs during detection, the output list is cleared and false is
    ///     returned. The method is thread-safe.
    /// </remarks>
    /// <param name="image">The image data in which to search for faces. Must not be null.</param>
    /// <param name="minScore">
    ///     The minimum confidence score required for a detected face to be included in the results. Must be between 0.0 and
    ///     1.0.
    /// </param>
    /// <param name="faces">
    ///     When this method returns, contains a list of detected face candidates with confidence scores greater than or
    ///     equal to <paramref name="minScore" />. The list is empty if no faces are detected or if detection fails.
    /// </param>
    /// <returns>
    ///     true if face detection was performed; otherwise, false. Returns false if face detection is disabled, the model
    ///     is unavailable, or an error occurs.
    /// </returns>
    private bool FaceTryDetect(ImageData image, float minScore, out List<RoiCandidate> faces)
    {
        faces = new List<RoiCandidate>(4);
        if (!IsFaceAvailable) return false;

        try
        {
            using var raw = new Mat();
            lock (_faceLock)
            {
                _faceModel.Detect(image.Mat, raw);
            }

            if (raw.IsEmpty || raw.Rows <= 0 || raw.Cols < 5) return true;

            var mat = !raw.IsContinuous ? raw.Clone() : raw;
            if (mat.Depth != DepthType.Cv32F)
            {
                var tmp = new Mat();
                mat.ConvertTo(tmp, DepthType.Cv32F);
                if (!ReferenceEquals(mat, raw)) mat.Dispose();
                mat = tmp;
            }

            var rows = mat.Rows;
            var cols = mat.Cols;
            var buf = new float[rows * cols];
            Marshal.Copy(mat.DataPointer, buf, 0, buf.Length);

            var border = new Rectangle(0, 0, image.Mat.Width, image.Mat.Height);
            for (var r = 0; r < rows; r++)
            {
                var i = r * cols;

                var x = (int)MathF.Round(buf[i + 0]);
                var y = (int)MathF.Round(buf[i + 1]);
                var w = (int)MathF.Round(buf[i + 2]);
                var h = (int)MathF.Round(buf[i + 3]);
                var score = buf[i + 4];

                if (score < minScore || w <= 0 || h <= 0) continue;

                var rect = Rectangle.Intersect(new Rectangle(x, y, w, h), border);
                if (rect.Width <= 0 || rect.Height <= 0) continue;

                faces.Add(new RoiCandidate(rect, score));
            }

            if (!ReferenceEquals(mat, raw)) mat.Dispose();
            return true;
        }
        catch
        {
            faces.Clear();
            return false;
        }
    }

    /// <summary>
    ///     Centers a fixed-size crop window on the anchor rectangle, shifting it as needed to stay within image bounds.
    /// </summary>
    /// <remarks>
    ///     If centering the window on the anchor would cause it to extend outside the border, the window
    ///     is shifted as necessary to remain fully within the border bounds.
    /// </remarks>
    /// <param name="anchor">
    ///     The rectangle on which to center the window. The window will be positioned so that its center aligns with the
    ///     center of this rectangle.
    /// </param>
    /// <param name="crop">
    ///     The window to be centered. Must be in the border
    ///     rectangle.
    /// </param>
    /// <param name="border">The rectangle representing the bounding area within which the window must be fully contained.</param>
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