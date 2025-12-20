namespace SlideGenerator.Framework.Image.Exceptions;

/// <summary>
///     Exception thrown when reading an image file fails.
/// </summary>
public class ReadImageFailed : Exception
{
    public ReadImageFailed(string sourceName)
        : base($"Failed to read image from: {sourceName}")
    {
        SourceName = sourceName;
    }

    public ReadImageFailed(string sourceName, Exception innerException)
        : base($"Failed to read image from: {sourceName}", innerException)
    {
        SourceName = sourceName;
    }

    public string SourceName { get; }
}