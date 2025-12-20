using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace SlideGenerator.Framework.Slide.Models;

/// <summary>
///     Represents a presentation working from a template, allowing slide generation.
/// </summary>
public sealed class WorkingPresentation : Presentation
{
    /// <summary>
    ///     Creates a working presentation.
    /// </summary>
    /// <param name="filePath">Destination path for the new presentation.</param>
    public WorkingPresentation(string filePath)
        : base(filePath, true)
    {
    }

    /// <summary>
    ///     Saves the presentation.
    /// </summary>
    public void Save()
    {
        GetPresentationPart().Presentation.Save();
    }

    /// <summary>
    ///     Copies a slide and inserts it at the specified position.
    /// </summary>
    /// <param name="slideRid">Relationship ID of the slide to copy.</param>
    /// <param name="position">
    ///     Position that new slide will have (1-based).
    ///     If below than 0 or greater than current total slides, appends to end.
    /// </param>
    /// <returns>The slide part of the copied slide.</returns>
    public SlidePart CopySlide(string slideRid, int position = -1)
    {
        var presentationPart = GetPresentationPart();
        var sourceSlide = GetSlidePart(slideRid);
        var newSlide = presentationPart.AddNewPart<SlidePart>();

        // Copy slide XML
        newSlide.FeedData(sourceSlide.GetStream());

        // Copy resource references
        foreach (var rel in sourceSlide.Parts)
        {
            var part = rel.OpenXmlPart;
            var rid = rel.RelationshipId;
            newSlide.AddPart(part, rid);
        }

        // Copy animations
        if (sourceSlide.Slide.Timing != null)
            newSlide.Slide.Timing = (Timing)sourceSlide.Slide.Timing.CloneNode(true);

        // Copy transitions
        if (sourceSlide.Slide.Transition != null)
            newSlide.Slide.Transition = (Transition)sourceSlide.Slide.Transition.CloneNode(true);

        // Add to slide list
        var slideIdList = GetSlideIdList();
        var maxId = slideIdList.ChildElements.Cast<SlideId>().Max(x => x.Id?.Value);
        var newSlideId = new SlideId
        {
            Id = maxId + 1 ?? 0,
            RelationshipId = presentationPart.GetIdOfPart(newSlide)
        };

        if (position <= 0 || position > slideIdList.Count())
            slideIdList.Append(newSlideId);
        else
            slideIdList.InsertAt(newSlideId, position - 1);

        return newSlide;
    }

    /// <summary>
    ///     Removes the slide at the specified position.
    /// </summary>
    /// <param name="position">Position of the slide to remove (1-based).</param>
    public void RemoveSlide(int position)
    {
        var slideIdList = GetSlideIdList();
        var slide = slideIdList.ChildElements.Cast<SlideId>().ElementAt(position - 1);
        slide?.Remove();
    }
}