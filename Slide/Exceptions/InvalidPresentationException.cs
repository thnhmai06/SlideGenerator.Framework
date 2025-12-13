namespace SlideGenerator.Core.Slide.Exceptions;

/// <summary>
///     Exception thrown when a presentation document is invalid or missing required parts.
/// </summary>
public class InvalidPresentationException(string filePath, string reason)
    : Exception($"Invalid presentation '{filePath}': {reason}")
{
    public string FilePath { get; } = filePath;
}