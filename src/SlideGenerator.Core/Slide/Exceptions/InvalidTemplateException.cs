namespace SlideGenerator.Core.Slide.Exceptions;

/// <summary>
///     Exception thrown when a template presentation does not have exactly one slide.
/// </summary>
public class InvalidTemplateException(string filePath, int slideCount)
    : ArgumentException($"Template '{filePath}' must have exactly one slide, but has {slideCount} slides.")
{
    public string FilePath { get; } = filePath;
    public int SlideCount { get; } = slideCount;
}