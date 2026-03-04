using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace SlideGenerator.Framework.Slide.Services.Presentation;

/// Reviewed by @thnhmai06 at 05/03/2026
public static partial class XmlPresentationService
{
    /// <summary>
    ///     Enumerates slide parts in display order.
    /// </summary>
    /// <param name="doc">The presentation document.</param>
    /// <returns>Ordered sequence of slide parts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when presentation structure is invalid.</exception>
    public static IEnumerable<SlidePart> EnumerateSlides(this PresentationDocument doc)
    {
        var presentationPart = doc.PresentationPart
                               ?? throw new InvalidOperationException(
                                   "Invalid presentation: missing presentation part.");
        var presentation = presentationPart.Presentation
                           ?? throw new InvalidOperationException("Invalid presentation: missing root presentation.");
        var slideIdList = presentation.SlideIdList
                          ?? throw new InvalidOperationException("Invalid presentation: missing slide list.");

        foreach (var slideId in slideIdList.Elements<SlideId>())
            yield return (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
    }
}