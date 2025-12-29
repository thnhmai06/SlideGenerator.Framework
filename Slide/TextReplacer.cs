using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using SlideGenerator.Framework.Slide.Models;
using Stubble.Core;
using Stubble.Core.Builders;
using DrawingText = DocumentFormat.OpenXml.Drawing.Text;

namespace SlideGenerator.Framework.Slide;

using ReplaceInstructions = Dictionary<string, string>;

/// <summary>
///     Provides text replacement functionality for slides using Mustache templates.
/// </summary>
public static partial class TextReplacer
{
    private const string TemplatePattern = @"\{\{\s*([^{}]+?)\s*\}\}"; // {{ placeholder }}

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

        var sanitized = SanitizeReplacements(replacements);
        var stubble = new StubbleBuilder()
            .Configure(settings => settings.SetEncodingFunction(value => value))
            .Build();
        var hasChanges = false;

        // Replace in presentation text
        var presentationTexts = Presentation.GetPresentationTexts(slidePart);
        foreach (var presText in presentationTexts)
        {
            var newText = await RenderSafeAsync(stubble, presText.Text, sanitized);
            if (newText != presText.Text)
            {
                presText.Text = newText;
                replacedCount++;
                hasChanges = true;
            }
        }

        // Replace in shape text bodies (handles placeholders split across runs).
        foreach (var shape in Presentation.GetShapes(slidePart))
        {
            var textBody = shape.TextBody;
            if (textBody is null) continue;

            var textRuns = textBody.Descendants<DrawingText>().ToList();
            if (textRuns.Count == 0) continue;

             var builder = new StringBuilder();
            foreach (var run in textRuns)
                builder.Append(run.Text);
            var original = builder.ToString();
            if (string.IsNullOrEmpty(original) || !original.Contains("{{", StringComparison.Ordinal))
                continue;

            var newText = await RenderSafeAsync(stubble, original, sanitized);
            if (newText == original) continue;

            textRuns[0].Text = newText;
            for (var i = 1; i < textRuns.Count; i++)
                textRuns[i].Text = string.Empty;

            replacedCount++;
            hasChanges = true;
        }

        if (hasChanges)
            slidePart.Slide.Save();

        return replacedCount;
    }

    private static async Task<string> RenderSafeAsync(
        StubbleVisitorRenderer stubble,
        string text,
        ReplaceInstructions replacements)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains("{{", StringComparison.Ordinal))
            return text;

        try
        {
            return await stubble.RenderAsync(text, replacements);
        }
        catch
        {
            return TemplateRegex().Replace(text, match =>
            {
                if (match.Groups.Count < 2) return match.Value;
                var key = match.Groups[1].Value.Trim();
                return replacements.TryGetValue(key, out var value) ? value : match.Value;
            });
        }
    }

    private static ReplaceInstructions SanitizeReplacements(ReplaceInstructions replacements)
    {
        var sanitized = new ReplaceInstructions(StringComparer.Ordinal);
        foreach (var (key, value) in replacements) sanitized[key] = SanitizeXmlValue(value);

        return sanitized;
    }

    private static string SanitizeXmlValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var buffer = new char[value.Length];
        var count = 0;
        foreach (var ch in value)
            if (XmlConvert.IsXmlChar(ch))
                buffer[count++] = ch;

        return new string(buffer, 0, count);
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