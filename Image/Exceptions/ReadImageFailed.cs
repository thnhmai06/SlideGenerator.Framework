namespace SlideGenerator.Framework.Image.Exceptions;

/// <summary>
///     Exception thrown when reading an image file fails.
/// </summary>
public class ReadImageFailed : Exception
{
    public ReadImageFailed(string filePath)
        : base($"Failed to read image from: {filePath}")
    {
        FilePath = filePath;
    }

    public ReadImageFailed(string filePath, Exception innerException)
        : base($"Failed to read image from: {filePath}", innerException)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}