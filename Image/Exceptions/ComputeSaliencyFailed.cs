namespace SlideGenerator.Framework.Image.Exceptions;

/// <summary>
///     Exception thrown when saliency computation fails for an image.
/// </summary>
public class ComputeSaliencyFailed : Exception
{
    public ComputeSaliencyFailed(string filePath)
        : base($"Failed to compute saliency map for: {filePath}")
    {
        FilePath = filePath;
    }

    public ComputeSaliencyFailed(string filePath, Exception innerException)
        : base($"Failed to compute saliency map for: {filePath}", innerException)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}