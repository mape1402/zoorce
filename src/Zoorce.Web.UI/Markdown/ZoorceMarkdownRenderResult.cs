using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Markdown;

/// <summary>Contains trusted HTML and navigation metadata generated from Markdown.</summary>
public sealed record ZoorceMarkdownRenderResult(
    IHtmlContent Html,
    IReadOnlyList<ZoorceMarkdownHeading> TableOfContents);
