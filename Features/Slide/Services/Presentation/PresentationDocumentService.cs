using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace SlideGenerator.Framework.Slide;

public static partial class PresentationDocumentService
{
    /// <summary>
    /// Opens an existing presentation document or creates a new one if the specified file does not exist.
    /// </summary>
    /// <remarks>If the specified file does not exist, a new presentation document is created at the given
    /// path. If the file exists, it is opened with the specified editability. The caller is responsible for disposing
    /// the returned PresentationDocument when it is no longer needed.</remarks>
    /// <param name="filePath">The path to the presentation file to open or create. If the file does not exist, a new presentation document is
    /// created at this location.</param>
    /// <param name="isEditable">true to open the document in editable mode; otherwise, false. This parameter is ignored if a new document is
    /// created.</param>
    /// <returns>A PresentationDocument representing the opened or newly created presentation.</returns>
    public static PresentationDocument OpenPresentationDocument(string filePath, bool isEditable = true)
    {
        return !Path.Exists(filePath)
            ? PresentationDocument.Create(filePath, PresentationDocumentType.Presentation)
            : PresentationDocument.Open(filePath, isEditable);
    }

    /// <summary>
    /// Creates a new presentation document based on the specified template file.
    /// </summary>
    /// <param name="filePath">The full path to the template file from which to create the presentation document. The file must exist and be a
    /// valid presentation template.</param>
    /// <returns>A <see cref="PresentationDocument"/> instance representing the newly created presentation based on the template.</returns>
    public static PresentationDocument OpenPresentationDocumentFromTemplate(string filePath)
    {
        return PresentationDocument.CreateFromTemplate(filePath);
    }

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