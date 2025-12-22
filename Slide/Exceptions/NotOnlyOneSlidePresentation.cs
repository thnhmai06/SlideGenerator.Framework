namespace SlideGenerator.Framework.Slide.Exceptions;

/// <summary>
///     The exception that is thrown when a template presentation must contain exactly one slide.
/// </summary>
/// <param name="filePath">The path to the presentation file that caused the exception.</param>
public class NotOnlyOneSlidePresentation(string filePath)
    : ArgumentException($"Presentation '{filePath}' must contain exactly one slide.", filePath)
{
    public string FilePath { get; } = filePath;
}
