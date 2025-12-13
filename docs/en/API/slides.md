# Slides API

## Templates and derived decks
- `TemplatePresentation` (requires exactly one slide). Main slide via `MainSlideRelationshipId` and `GetMainSlidePart()`.
- `DerivedPresentation` copies a template. `CopySlide(slideRid, position)` clones; `RemoveSlide(position)` deletes; `Save()` persists.

## Text replacement
- Discover placeholders: `TextReplacer.ScanPlaceholders(string|SlidePart)` returns Mustache token names.
- Replace text: `TextReplacer.Replace(...)` or `ReplaceAsync(...)` renders with a `Dictionary<string,string>`.

## Shapes and pictures
- Helpers to inspect content: `Presentation.GetPictures(...)`, `GetShapes(...)`, `GetPresentationTexts(...)`, `GetDrawingTexts(...)`.
- Image insertion is covered in [api-images.md](api-images.md).

## Dispose
- Dispose `TemplatePresentation` and `DerivedPresentation` to release OpenXML resources.
