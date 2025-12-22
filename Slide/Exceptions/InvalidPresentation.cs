namespace SlideGenerator.Framework.Slide.Exceptions;

/// <summary>
///     Exception thrown when a presentation document is invalid or missing required parts.
/// </summary>
public class InvalidPresentation(string filePath, string reason)
    : ArgumentException($"Invalid presentation '{filePath}': {reason}")
{
    public string FilePath { get; } = filePath;
}