using DocumentFormat.OpenXml;
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

    internal WorkingPresentation(PresentationDocument doc, string filePath)
        : base(doc, filePath)
    {
    }

    /// <summary>
    ///     Saves the presentation.
    /// </summary>
    public void Save()
    {
        GetPresentationPart()!.Presentation!.Save();
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
        var newSlide = presentationPart!.AddNewPart<SlidePart>();

        // clone slide XML (contains transition/timing)
        newSlide.Slide = (DocumentFormat.OpenXml.Presentation.Slide)sourceSlide!.Slide!.CloneNode(true);

        // old rId -> new rId
        var ridMap = new Dictionary<string, string>(StringComparer.Ordinal);

        // reuse OpenXmlPart (image/layout/chart/...) but re-create relationships (new rId)
        // avoid copying NotesSlidePart: sharing notes across slides often corrupts package
        foreach (var rel in sourceSlide.Parts)
        {
            if (rel.OpenXmlPart is NotesSlidePart)
                continue;

            var oldRid = rel.RelationshipId;
            if (string.IsNullOrWhiteSpace(oldRid))
                continue;

            var added = newSlide.AddPart(rel.OpenXmlPart);
            var newRid = newSlide.GetIdOfPart(added);

            ridMap[oldRid] = newRid;
        }

        // reuse media (video/audio/media) by re-creating relationships and remap rId
        var unsupportedDataRelIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var dpr in sourceSlide.DataPartReferenceRelationships)
        {
            var oldRid = dpr.Id; // rId in slide xml
            if (string.IsNullOrWhiteSpace(oldRid))
                continue;

            if (dpr.DataPart is MediaDataPart media)
            {
                // Create a new relationship (new rId), then remap slide XML to that id
                var newRid = dpr switch
                {
                    VideoReferenceRelationship => newSlide.AddVideoReferenceRelationship(media).Id,
                    AudioReferenceRelationship => newSlide.AddAudioReferenceRelationship(media).Id,
                    _ => newSlide.AddMediaReferenceRelationship(media).Id
                };

                ridMap[oldRid] = newRid;
            }
            else
            {
                // Other DataPart kinds (OLE/controls/embedded) are not safely addable via public SDK API
                unsupportedDataRelIds.Add(oldRid);
            }
        }

        // external relationships: re-create and remap
        foreach (var ext in sourceSlide.ExternalRelationships)
        {
            var oldRid = ext.Id;
            if (string.IsNullOrWhiteSpace(oldRid))
                continue;

            var created = newSlide.AddExternalRelationship(ext.RelationshipType, ext.Uri);
            ridMap[oldRid] = created.Id;
        }

        // hyperlink relationships: re-create and remap
        foreach (var link in sourceSlide.HyperlinkRelationships)
        {
            var oldRid = link.Id;
            if (string.IsNullOrWhiteSpace(oldRid))
                continue;

            var created = newSlide.AddHyperlinkRelationship(link.Uri, link.IsExternal);
            ridMap[oldRid] = created.Id;
        }

        // strip unsupported DataPartReferenceRelationship to avoid dangling rIds -> PowerPoint repair/corrupt
        if (unsupportedDataRelIds.Count > 0)
            RemoveElementsReferencingRelIds(newSlide.Slide, unsupportedDataRelIds);

        // rewrite r:id / r:embed / r:link in slide XML to the newly created relationship ids
        RemapRelIds(newSlide.Slide, ridMap);

        newSlide.Slide.Save();

        // add slide to SlideIdList
        var slideIdList = GetSlideIdList()!;
        uint nextId = 256;
        var hasIds = false;
        uint maxId = 0;
        foreach (var slideId in slideIdList.ChildElements.OfType<SlideId>())
        {
            var idValue = slideId.Id?.Value;
            if (!idValue.HasValue) continue;
            if (!hasIds || idValue.Value > maxId)
            {
                maxId = idValue.Value;
                hasIds = true;
            }
        }

        if (hasIds) nextId = maxId + 1;

        var newSlideId = new SlideId
        {
            Id = nextId,
            RelationshipId = presentationPart.GetIdOfPart(newSlide)
        };

        if (position <= 0 || position > slideIdList.Count())
            slideIdList.Append(newSlideId);
        else
            slideIdList.InsertAt(newSlideId, position - 1);

        presentationPart.Presentation!.Save();
        return newSlide;
    }

    private static void RemapRelIds(OpenXmlElement root, Dictionary<string, string> ridMap)
    {
        const string relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        foreach (var el in root.Descendants())
        {
            var attrs = el.GetAttributes();
            if (attrs.Count == 0) continue;

            var changed = false;

            for (var i = 0; i < attrs.Count; i++)
            {
                var a = attrs[i];
                if (a.NamespaceUri != relNs) continue;
                if (a.LocalName is not ("id" or "embed" or "link")) continue;
                if (string.IsNullOrEmpty(a.Value)) continue;

                if (ridMap.TryGetValue(a.Value, out var newRid) &&
                    !string.Equals(a.Value, newRid, StringComparison.Ordinal))
                {
                    attrs[i] = new OpenXmlAttribute(a.Prefix, a.LocalName, a.NamespaceUri, newRid);
                    changed = true;
                }
            }

            if (changed)
                el.SetAttributes(attrs);
        }
    }

    private static void RemoveElementsReferencingRelIds(OpenXmlElement root, HashSet<string> relIds)
    {
        // r:id, r:embed, r:link are in namespace relationships
        const string relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        var toRemove = root
            .Descendants()
            .Where(el => el.GetAttributes().Any(a =>
                a is { NamespaceUri: relNs, LocalName: "id" or "embed" or "link", Value: not null } &&
                relIds.Contains(a.Value)))
            .ToList();

        foreach (var el in toRemove)
            el.Remove();
    }


    /// <summary>
    ///     Removes the slide at the specified position.
    /// </summary>
    /// <param name="position">Position of the slide to remove (1-based).</param>
    public void RemoveSlide(int position)
    {
        var slideIdList = GetSlideIdList();
        var slide = slideIdList!.ChildElements.Cast<SlideId>().ElementAt(position - 1);
        slide?.Remove();
    }
}