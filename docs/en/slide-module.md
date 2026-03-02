# Slide Module Documentation

[🇻🇳 Vietnamese Version](../vi/slide-module.md)

## Overview

The Slide module provides high-level abstractions for PowerPoint manipulation. It handles template loading, slide cloning, text replacement, and image insertion—the core operations needed for programmatic presentation generation.

## Architecture

### Core Components

#### TemplatePresentation

Represents a template PowerPoint file (read-only):

```csharp
namespace SlideGenerator.Framework.Features.Slide.Models;

public sealed class TemplatePresentation : IDisposable
{
    /// <summary>
    ///     Loads a template presentation from the specified file path.
    /// </summary>
    /// <param name="path">Path to the .pptx template file.</param>
    public TemplatePresentation(string path);
    
    /// <summary>
    ///     Gets the relationship ID of the main slide (slide 1).
    /// </summary>
    public string MainSlideRelationshipId { get; }
    
    /// <summary>
    ///     Gets all picture/image shapes in the template.
    /// </summary>
    /// <returns>List of shape IDs that contain images.</returns>
    public List<uint> GetAllPreviewImageShapes();
    
    /// <summary>
    ///     Gets all placeholder shapes in the template.
    /// </summary>
    /// <returns>List of shape IDs for text placeholders.</returns>
    public List<uint> GetAllPlaceholders();
    
    public void Dispose();
}
```

#### WorkingPresentation

Represents the output PowerPoint file (read-write):

```csharp
public sealed class WorkingPresentation : IDisposable
{
    /// <summary>
    ///     Gets the internal PresentationDocument for advanced operations.
    /// </summary>
    public PresentationDocument Document { get; }
    
    /// <summary>
    ///     Clones a slide from the template.
    /// </summary>
    /// <param name="templateRelationshipId">The slide ID to clone from.</param>
    /// <returns>The slide part of the cloned slide in the working presentation.</returns>
    public SlidePart CopySlide(string templateRelationshipId);
    
    /// <summary>
    ///     Saves the presentation to file.
    /// </summary>
    public void Save();
    
    public void Dispose();
}
```

### Text Replacement

Replace placeholder text marked with `{{Key}}` syntax:

```csharp
namespace SlideGenerator.Framework.Features.Slide.Services.Replacer;

public static class TextReplacer
{
    /// <summary>
    ///     Replaces all text placeholders in a slide.
    /// </summary>
    /// <param name="slidePart">The slide to search and replace.</param>
    /// <param name="replacements">Dictionary of key → value for replacement.</param>
    public static Task ReplaceAsync(
        SlidePart slidePart,
        IReadOnlyDictionary<string, string> replacements);
}
```

### Image Replacement

Replace images by shape ID:

```csharp
public static class ImageReplacer
{
    /// <summary>
    ///     Replaces an image in a specific shape.
    /// </summary>
    /// <param name="slidePart">The slide containing the image shape.</param>
    /// <param name="shape">The shape object containing the image.</param>
    /// <param name="imageStream">Stream of the new image (PNG, JPG, etc.).</param>
    public static void ReplaceImage(
        SlidePart slidePart,
        Shape shape,
        Stream imageStream);
}
```

### Shape Finding

Utilities for locating shapes in slides:

```csharp
public static class ShapeService
{
    /// <summary>
    ///     Finds a shape by its unique ID.
    /// </summary>
    /// <param name="slidePart">The slide to search.</param>
    /// <param name="shapeId">The shape ID (from template discovery).</param>
    /// <returns>The shape, or null if not found.</returns>
    public static Shape? FindShapeById(SlidePart slidePart, uint shapeId);
    
    /// <summary>
    ///     Finds a picture/image shape by ID.
    /// </summary>
    /// <param name="slidePart">The slide to search.</param>
    /// <param name="shapeId">The shape ID.</param>
    /// <returns>The picture shape, or null if not found.</returns>
    public static Picture? FindPictureById(SlidePart slidePart, uint shapeId);
}
```

## Usage

### Basic Generation Flow

```csharp
using SlideGenerator.Framework.Features.Slide.Models;
using SlideGenerator.Framework.Features.Slide.Services;

public class SlideGenerator
{
    public void GeneratePresentation(
        string templatePath,
        string outputPath,
        Dictionary<string, string> textReplacements,
        Dictionary<uint, string> imageReplacements)
    {
        // 1. Load template
        using var template = new TemplatePresentation(templatePath);
        
        // 2. Create output from template
        using var working = template.SaveAs(outputPath);
        
        // 3. Clone slide
        var slidePart = working.CopySlide(template.MainSlideRelationshipId);
        
        // 4. Replace text
        await TextReplacer.ReplaceAsync(slidePart, textReplacements);
        
        // 5. Replace images
        foreach (var (shapeId, imagePath) in imageReplacements)
        {
            var shape = ShapeService.FindShapeById(slidePart, shapeId);
            if (shape != null)
            {
                using var imgStream = File.OpenRead(imagePath);
                ImageReplacer.ReplaceImage(slidePart, shape, imgStream);
            }
        }
        
        // 6. Save
        working.Save();
    }
}
```

### Step-by-Step Walkthrough

#### 1. Load Template

```csharp
using var template = new TemplatePresentation("template.pptx");

// Discover what can be modified
var imageShapes = template.GetAllPreviewImageShapes();
var placeholders = template.GetAllPlaceholders();

Console.WriteLine($"Image shapes: {string.Join(", ", imageShapes)}");
Console.WriteLine($"Placeholders: {string.Join(", ", placeholders)}");
```

#### 2. Create Working Presentation

```csharp
using var working = template.SaveAs("output.pptx");
// Creates a copy for modifications
// Template remains unchanged
```

#### 3. Clone Slide

```csharp
// Get template slide ID
var slideId = template.MainSlideRelationshipId;

// Clone in working presentation
var slidePart = working.CopySlide(slideId);
```

#### 4. Replace Text Placeholders

```csharp
var replacements = new Dictionary<string, string>
{
    ["{{Name}}"] = "Alice Johnson",
    ["{{Title}}"] = "Senior Engineer",
    ["{{Date}}"] = DateTime.Now.ToString("yyyy-MM-dd")
};

await TextReplacer.ReplaceAsync(slidePart, replacements);
```

#### 5. Replace Images

```csharp
// Find image shape
var pictureShape = ShapeService.FindPictureById(slidePart, 4);
if (pictureShape != null)
{
    using var imageStream = File.OpenRead("photo.jpg");
    ImageReplacer.ReplaceImage(slidePart, pictureShape, imageStream);
}
```

#### 6. Save Presentation

```csharp
working.Save();
// Output.pptx is now ready with all changes
```

## Template Design

### Text Placeholders

Use `{{Key}}` syntax in text boxes:

```
Template text: "Hello {{Name}}"
Replacement:   {"Name": "Alice"}
Result:        "Hello Alice"
```

**Rules:**
- Case-sensitive matching
- Supports multiple placeholders per text box
- ✅ `"Name: {{Name}}, Title: {{Title}}"` → All replaced
- ✅ Nested: `"{{Prefix}} {{Name}}"` → Works
- ❌ Regex patterns: Not supported

### Image Placeholders

Mark images with Shape IDs:

1. In PowerPoint template, insert placeholder image
2. Right-click → Format Picture → Name/ID
3. Note the shape ID (usually numeric)
4. Use `ImageReplacer` with that ID

**Important:**
- Each image must have unique shape ID
- Can be discovered via `template.GetAllPreviewImageShapes()`
- Use appropriate image format (PNG recommended for transparency)

## Advanced Usage

### Multi-Slide Generation

```csharp
public class MultiSlideGenerator
{
    public void GenerateMultipleSlides(
        string templatePath,
        string outputPath,
        List<Dictionary<string, string>> rowsData)
    {
        using var template = new TemplatePresentation(templatePath);
        using var working = template.SaveAs(outputPath);
        
        foreach (var rowData in rowsData)
        {
            // Clone slide for each row
            var slidePart = working.CopySlide(template.MainSlideRelationshipId);
            
            // Replace with row-specific data
            await TextReplacer.ReplaceAsync(slidePart, rowData);
            
            // Handle images if needed
            if (rowData.TryGetValue("ImagePath", out var imagePath))
            {
                var shape = ShapeService.FindPictureById(slidePart, 4);
                if (shape != null)
                {
                    using var stream = File.OpenRead(imagePath);
                    ImageReplacer.ReplaceImage(slidePart, shape, stream);
                }
            }
        }
        
        working.Save();
    }
}
```

### Conditional Replacements

```csharp
public Dictionary<string, string> BuildReplacements(Dictionary<string, object> rowData)
{
    var replacements = new Dictionary<string, string>();
    
    foreach (var (key, value) in rowData)
    {
        if (value != null)
        {
            replacements[key] = value.ToString();
        }
        else
        {
            // Empty cell → use default
            replacements[key] = "[Not Available]";
        }
    }
    
    return replacements;
}
```

### Error Handling

```csharp
try
{
    using var template = new TemplatePresentation(templatePath);
    using var working = template.SaveAs(outputPath);
    
    var slidePart = working.CopySlide(template.MainSlideRelationshipId);
    
    var shape = ShapeService.FindShapeById(slidePart, 999);
    if (shape == null)
        throw new InvalidOperationException("Shape 999 not found in template");
    
    await TextReplacer.ReplaceAsync(slidePart, replacements);
    working.Save();
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Template not found: {ex.FileName}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid template structure: {ex.Message}");
}
```

## Performance Considerations

### Cloning Cost

- **First clone**: ~50-200ms (depends on slide complexity)
- **Subsequent clones**: ~30-100ms (cached resources)
- **Optimization**: Clone once, reuse if possible

### Image Replacement Cost

- **Small images** (< 1MB): ~10-50ms
- **Large images** (> 5MB): ~100-500ms
- **Optimization**: Pre-compress/resize images before insertion

### Memory Usage

- Template: ~1-10MB in memory (depends on slide)
- Working copy: ~2-20MB
- Optimization: Dispose workbooks after each generation

## Best Practices

### 1. Template Validation

```csharp
public void ValidateTemplate(string templatePath)
{
    using var template = new TemplatePresentation(templatePath);
    
    var imageShapes = template.GetAllPreviewImageShapes();
    var placeholders = template.GetAllPlaceholders();
    
    if (imageShapes.Count == 0)
        throw new InvalidOperationException("Template has no image placeholders");
    
    if (placeholders.Count == 0)
        throw new InvalidOperationException("Template has no text placeholders");
}
```

### 2. Resource Cleanup

```csharp
// ✅ Good: Using statements
using var template = new TemplatePresentation(templatePath);
using var working = template.SaveAs(outputPath);
{
    // Work with presentation
}
// All resources cleaned up

// ❌ Bad: No cleanup
var template = new TemplatePresentation(templatePath);
var working = template.SaveAs(outputPath);
// File handles not released
```

### 3. Shape ID Management

```csharp
public class ShapeMapper
{
    private readonly Dictionary<string, uint> _shapeIds = new();
    
    public void DiscoverShapes(string templatePath)
    {
        using var template = new TemplatePresentation(templatePath);
        
        _shapeIds["MainImage"] = template.GetAllPreviewImageShapes()[0];
        _shapeIds["SecondaryImage"] = template.GetAllPreviewImageShapes()[1];
    }
    
    public uint GetShapeId(string name) => _shapeIds[name];
}
```

## Limitations

- ❌ Multiple template slides: Only slide 1 is cloned
- ❌ Complex animations: Preserved but may not work as expected
- ❌ VBA macros: Stripped during save
- ✅ Text formatting: Preserved during replacement
- ✅ Image positioning: Preserved during replacement
- ✅ Complex shapes: Supported

## Thread Safety

- ❌ `TemplatePresentation` and `WorkingPresentation` are **NOT thread-safe**
- ✅ Create separate instances per thread
- ✅ Multiple threads can work on different presentations simultaneously

---

Next: [Image Module](image-module.md) | [Overview](overview.md)

