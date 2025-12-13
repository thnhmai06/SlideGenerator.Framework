namespace SlideGenerator.Framework.Slide.Exceptions;

/// <summary>
///     Exception thrown when a slide element does not contain expected image data.
/// </summary>
public class ImageNotFoundInShapeException(string shapeId, string reason)
    : InvalidOperationException($"Shape '{shapeId}' does not contain image: {reason}")
{
    public string ShapeId { get; } = shapeId;
}