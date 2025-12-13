namespace SlideGenerator.Core.Image.Exceptions;

/// <summary>
///     Exception thrown when reading an image file fails.
/// </summary>
public class ImageReadException : Exception
{
    public ImageReadException(string filePath)
        : base($"Failed to read image from: {filePath}")
    {
        FilePath = filePath;
    }

    public ImageReadException(string filePath, Exception innerException)
        : base($"Failed to read image from: {filePath}", innerException)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}