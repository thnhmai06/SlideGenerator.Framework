namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Represents image shape data extracted from a slide.
/// </summary>
/// <param name="Name">The name of the shape.</param>
/// <param name="ImageBytes">The image data as a byte array.</param>
public record ImageShapePreview(string Name, byte[] ImageBytes);