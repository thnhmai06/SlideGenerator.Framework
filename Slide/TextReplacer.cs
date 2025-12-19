using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using SlideGenerator.Framework.Slide.Models;
using Stubble.Core.Builders;

namespace SlideGenerator.Framework.Slide;

using ReplaceInstructions = Dictionary<string, string>;

/// <summary>
///     Provides text replacement functionality for slides using Mustache templates.
/// </summary>
public static partial class TextReplacer
{
    private const string TemplatePattern = @"\{\{([\w\d\s]+)\}\}";

    [GeneratedRegex(TemplatePattern)]
    private static partial Regex TemplateRegex();

    /// <summary>
    ///     Scans text for Mustache template placeholders.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <returns>A set of placeholder names found in the text.</returns>
    public static HashSet<string> ScanPlaceholders(string text)
    {
        HashSet<string> templates = [];
        var matches = TemplateRegex().Matches(text);
        foreach (Match match in matches)
            if (match.Groups.Count > 1)
                templates.Add(match.Groups[1].Value.Trim());

        return templates;
    }

    /// <summary>
    ///     Scans a slide for all Mustache template placeholders.
    /// </summary>
    /// <param name="slidePart">The slide part to scan.</param>
    /// <returns>A set of placeholder names found in the slide.</returns>
    public static HashSet<string> ScanPlaceholders(SlidePart slidePart)
    {
        HashSet<string> templates = [];

        // Scan presentation text (quick)
        var allText = slidePart.Slide.InnerText;
        templates.UnionWith(ScanPlaceholders(allText));

        // Scan drawing text
        var drawingTexts = Presentation.GetDrawingTexts(slidePart);
        foreach (var drawingText in drawingTexts)
        {
            var rawText = drawingText.Text;
            templates.UnionWith(ScanPlaceholders(rawText));
        }

        return templates;
    }

    /// <summary>
    ///     Replaces Mustache template placeholders in a slide with the provided values.
    /// </summary>
    /// <param name="slidePart">The slide part to modify.</param>
    /// <param name="replacements">Dictionary mapping placeholder names to replacement values.</param>
    /// <returns>The number of replacements made.</returns>
    public static async Task<uint> ReplaceAsync(SlidePart slidePart, ReplaceInstructions replacements)
    {
        uint replacedCount = 0;
        if (replacements.Count == 0) return replacedCount;

        var stubble = new StubbleBuilder().Build();

        // Replace in presentation text
        var presentationTexts = Presentation.GetPresentationTexts(slidePart);
        foreach (var presText in presentationTexts)
        {
            var newText = await stubble.RenderAsync(presText.Text, replacements);
            if (newText != presText.Text)
            {
                presText.Text = newText;
                replacedCount++;
            }
        }

        // Replace in drawing text
        var drawingTexts = Presentation.GetDrawingTexts(slidePart);
        foreach (var drawingText in drawingTexts)
        {
            var newText = await stubble.RenderAsync(drawingText.Text, replacements);
            if (newText != drawingText.Text)
            {
                drawingText.Text = newText;
                replacedCount++;
            }
        }

        return replacedCount;
    }

    /// <summary>
    ///     Replaces Mustache template placeholders in a slide with the provided values (synchronous).
    /// </summary>
    /// <param name="slidePart">The slide part to modify.</param>
    /// <param name="replacements">Dictionary mapping placeholder names to replacement values.</param>
    /// <returns>The number of replacements made.</returns>
    public static uint Replace(SlidePart slidePart, ReplaceInstructions replacements)
    {
        return ReplaceAsync(slidePart, replacements).GetAwaiter().GetResult();
    }
}