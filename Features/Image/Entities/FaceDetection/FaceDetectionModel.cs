using Emgu.CV;
using SlideGenerator.Framework.Image.Models.FaceDetection;

namespace SlideGenerator.Framework.Image.Entities.FaceDetection;

/// <summary>
///     A face detector model.
/// </summary>
public abstract class FaceDetectorModel : IDisposable
{
    /// <summary>
    ///     Gets a value indicating whether the face detection model is available for use.
    /// </summary>
    public abstract bool IsModelAvailable { get; }

    /// <summary>
    ///     Gets the options used to configure face detection behavior.
    /// </summary>
    public FaceDetectionOptions Options { get; init; } = new();

    public abstract void Dispose();

    /// <summary>
    ///     Initializes the face detection model asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous initialization operation. The task result is <see langword="true" /> if the
    ///     model was successfully initialized; otherwise, <see langword="false" />.
    /// </returns>
    public abstract Task<bool> InitAsync();

    /// <summary>
    ///     Deinitializes the face detection model asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous deinitialization operation. The task result is <see langword="true" /> if
    ///     the deinitialization completed successfully; otherwise, <see langword="false" />.
    /// </returns>
    public abstract Task<bool> DeInitAsync();

    /// <summary>
    ///     Attempts to detect faces in the specified image.
    /// </summary>
    /// <remarks>
    ///     The method returns a list of detected face candidates. If the face model cannot be initialized
    ///     or an error occurs during detection, an empty list is returned.
    /// </remarks>
    /// <param name="mat">The mat in which to search for faces. Must not be null.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a list of detected
    ///     face candidates. The list is empty if no faces are detected or if detection fails.
    /// </returns>
    public abstract Task<List<Face>> DetectAsync(Mat mat);
}