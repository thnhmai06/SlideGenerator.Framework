using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace SlideGenerator.Framework.Slide;

/// <summary>
///     Provides services for managing presentation documents, including slide cloning and template extraction.
/// </summary>
public static partial class PresentationService
{
    /// <summary>
    ///     Gets the relationship ID of a slide at the specified index.
    /// </summary>
    /// <param name="doc">The presentation document.</param>
    /// <param name="index">The 0-based index of the slide.</param>
    /// <returns>The relationship ID of the slide.</returns>
    public static SlideId? GetSlideId(PresentationDocument doc, int index)
    {
        var slideIdList = doc.PresentationPart?.Presentation?.SlideIdList;
        var slideIds = slideIdList?.ChildElements;
        return (SlideId?)slideIds?[index];
    }
}