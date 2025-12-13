namespace SlideGenerator.Core.Image.Exceptions;

/// <summary>
///     Exception thrown when saliency computation fails for an image.
/// </summary>
public class SaliencyComputationException : Exception
{
    public SaliencyComputationException(string filePath)
        : base($"Failed to compute saliency map for: {filePath}")
    {
        FilePath = filePath;
    }

    public SaliencyComputationException(string filePath, Exception innerException)
        : base($"Failed to compute saliency map for: {filePath}", innerException)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}