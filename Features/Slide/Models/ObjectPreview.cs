namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Represents presentation object data and its preview.
/// </summary>
/// <param name="Id">The unique identifier of the object.</param>
/// <param name="Name">The name of the object.</param>
/// <param name="ImageBytes">The preview image data as a byte array.</param>
public record ObjectPreview(uint Id, string Name, byte[] ImageBytes);