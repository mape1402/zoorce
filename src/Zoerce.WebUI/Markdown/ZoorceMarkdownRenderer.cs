using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Html;

namespace Zoerce.WebUI.Markdown;

/// <summary>Markdig-based Markdown renderer with a conservative HTML and link policy.</summary>
internal sealed class ZoorceMarkdownRenderer : IZoorceMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .Build();

    /// <inheritdoc />
    public ZoorceMarkdownRenderResult Render(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var document = Markdig.Markdown.Parse(markdown, Pipeline);
        var headings = BuildTableOfContents(document);
        using StringWriter writer = new();
        var renderer = new HtmlRenderer(writer)
        {
            LinkRewriter = RewriteLink
        };
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        return new ZoorceMarkdownRenderResult(new HtmlString(writer.ToString()), headings);
    }

    private static IReadOnlyList<ZoorceMarkdownHeading> BuildTableOfContents(MarkdownDocument document)
    {
        List<ZoorceMarkdownHeading> headings = [];
        Dictionary<string, int> usedIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            var text = ExtractText(heading.Inline).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var attributes = heading.GetAttributes();
            var id = UniqueSlug(string.IsNullOrWhiteSpace(attributes.Id) ? text : attributes.Id, usedIds);
            attributes.Id = id;
            headings.Add(new ZoorceMarkdownHeading(id, text, heading.Level));
        }

        return headings;
    }

    private static string RewriteLink(string target)
        => IsSafeLink(target) ? target : "#";

    private static bool IsSafeLink(string target)
    {
        if (string.IsNullOrWhiteSpace(target) || target.StartsWith('#') || target.StartsWith("./") || target.StartsWith("../"))
        {
            return true;
        }

        if (target.StartsWith('/'))
        {
            return !target.StartsWith("//", StringComparison.Ordinal);
        }

        return Uri.TryCreate(target, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeMailto);
    }

    private static string UniqueSlug(string source, IDictionary<string, int> usedIds)
    {
        var builder = new StringBuilder();
        var pendingDash = false;
        foreach (var character in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                if (pendingDash && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(character);
                pendingDash = false;
            }
            else
            {
                pendingDash = builder.Length > 0;
            }
        }

        var candidate = builder.Length == 0 ? "section" : builder.ToString();
        if (!usedIds.TryGetValue(candidate, out var occurrences))
        {
            usedIds[candidate] = 1;
            return candidate;
        }

        usedIds[candidate] = occurrences + 1;
        return $"{candidate}-{occurrences}";
    }

    private static string ExtractText(ContainerInline? inline)
    {
        if (inline is null)
        {
            return string.Empty;
        }

        StringBuilder result = new();
        foreach (var child in inline)
        {
            AppendInlineText(result, child);
        }

        return result.ToString();
    }

    private static void AppendInlineText(StringBuilder result, Inline inline)
    {
        switch (inline)
        {
            case LiteralInline literal:
                result.Append(literal.Content.Text.Substring(literal.Content.Start, literal.Content.Length));
                break;
            case CodeInline code:
                result.Append(code.Content);
                break;
            case ContainerInline container:
                foreach (var child in container)
                {
                    AppendInlineText(result, child);
                }

                break;
        }
    }
}
