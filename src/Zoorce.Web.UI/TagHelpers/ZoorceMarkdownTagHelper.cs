using Microsoft.AspNetCore.Razor.TagHelpers;
using Zoorce.Web.UI.Markdown;

namespace Zoorce.Web.UI.TagHelpers;

/// <summary>Renders safe Markdown through a semantic article element.</summary>
[HtmlTargetElement("zoorce-markdown")]
public sealed class ZoorceMarkdownTagHelper(IZoorceMarkdownRenderer renderer) : TagHelper
{
    /// <summary>Gets or sets the Markdown source.</summary>
    [HtmlAttributeName("markdown")]
    public string Markdown { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional additional CSS class.</summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var rendered = renderer.Render(Markdown);
        output.TagName = "article";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", string.IsNullOrWhiteSpace(CssClass) ? "z-markdown" : $"z-markdown {CssClass}");
        output.Content.SetHtmlContent(rendered.Html);
    }
}
