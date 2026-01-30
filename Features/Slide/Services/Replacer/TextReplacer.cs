using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Stubble.Core;
using Stubble.Core.Builders;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace SlideGenerator.Framework.Slide.Services;

using ReplaceInstructions = Dictionary<string, string>;

/// <summary>
///     Provides text replacement functionality for slides on <see cref="MustacheTemplate" />.
/// </summary>
public static partial class TextReplacer
{
    private const string MustacheTemplatePattern = @"\{\{\s*([^{}]+?)\s*\}\}"; // {{ placeholder }}

    private static readonly StubbleVisitorRenderer Stubble = new StubbleBuilder()
        .Configure(settings => settings.SetEncodingFunction(value => value))
        .Build();

    [GeneratedRegex(MustacheTemplatePattern)]
    private static partial Regex MustacheTemplate();

    /// <summary>
    ///     Scans text for Mustache template placeholders.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <returns>A set of placeholder names found in the text.</returns>
    public static HashSet<string> ScanPlaceholders(string text)
    {
        HashSet<string> templates = [];
        var matches = MustacheTemplate().Matches(text);
        foreach (Match match in matches)
            if (match.Groups.Count > 1)
                templates.Add(match.Groups[1].Value.Trim());

        return templates;
    }

    /// <summary>
    ///     Scans a slide for Mustache placeholders and maps them to their containing shapes.
    /// </summary>
    /// <param name="slidePart">The slide part to scan.</param>
    /// <returns>A list of tuples linking the shape preview to the found placeholder.</returns>
    public static List<(Shape Shape, string Placeholder)> ScanPlaceholders(SlidePart slidePart)
    {
        var result = new List<(Shape, string)>();
        if (slidePart.Slide == null) return result;

        var shapes = slidePart.Slide!.Descendants<Shape>().Where(s => s.TextBody != null);
        foreach (var shape in shapes)
        {
            var sb = new StringBuilder();
            foreach (var paragraph in shape.TextBody!.Descendants<Paragraph>())
            {
                foreach (var run in paragraph.Descendants<Run>())
                    sb.Append(run.Text?.Text ?? string.Empty);
                sb.Append(Environment.NewLine);
            }

            var fullText = sb.ToString();
            var foundPlaceholders = ScanPlaceholders(fullText);
            if (foundPlaceholders.Count <= 0) continue;

            result.AddRange(foundPlaceholders.Select(placeholder => (shape, placeholder)));
        }

        return result;
    }

    ///
    public static async Task<List<(Shape Shape, string Old, string New)>>
        ReplaceTextAsync(SlidePart slidePart, ReplaceInstructions instructions)
    {
        var changeLog = new List<(Shape Shape, string Old, string New)>();

        if (instructions.Count == 0) return changeLog;
        var sanitized = SanitizeXmlValue(instructions);

        var foundPlaceholders = ScanPlaceholders(slidePart);
        var targetShapes = new HashSet<Shape>(foundPlaceholders.Select(p => p.Shape));

        foreach (var shape in targetShapes)
            // for follow paragraph to save Bullet point
        foreach (var paragraph in shape.TextBody!.Descendants<Paragraph>())
        {
            var runs = paragraph.Descendants<Run>().ToList();
            if (runs.Count == 0) continue;

            var builder = new StringBuilder();
            foreach (var run in runs) builder.Append(run.Text?.Text ?? string.Empty);
            var originalText = builder.ToString();

            var newText = await RenderSafeAsync(Stubble, originalText, sanitized);
            if (newText == originalText) continue;
            var keysInPara = ScanPlaceholders(originalText);
            foreach (var key in keysInPara)
                if (instructions.TryGetValue(key, out var val))
                    changeLog.Add((shape, key, val));

            runs[0].Text ??= new Text();
            runs[0].Text!.Text = newText;

            for (var i = 1; i < runs.Count; i++)
                if (runs[i].Text != null)
                    runs[i].Text!.Text = string.Empty;
        }

        return changeLog;
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
            // use stubble
            return await stubble.RenderAsync(text, replacements);
        }
        catch
        {
            // use regex
            return MustacheTemplate().Replace(text, match =>
            {
                if (match.Groups.Count < 2) return match.Value;
                var key = match.Groups[1].Value.Trim();
                return replacements.TryGetValue(key, out var value) ? value : match.Value;
            });
        }
    }

    private static ReplaceInstructions SanitizeXmlValue(ReplaceInstructions replacements)
    {
        var sanitized = new ReplaceInstructions(StringComparer.Ordinal);
        foreach (var (key, value) in replacements)
            sanitized[key] = SanitizeXmlValue(value);

        return sanitized;
    }

    private static string SanitizeXmlValue(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.Any(XmlConvert.IsXmlChar)) return value;

        var buffer = new char[value.Length];
        var count = 0;
        foreach (var ch in value.Where(XmlConvert.IsXmlChar))
            buffer[count++] = ch;

        return new string(buffer, 0, count);
    }
}