using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Zoorce.Web.UI.TagHelpers;

/// <summary>A reusable animated help companion based on Valky.</summary>
[HtmlTargetElement("zoorce-valky")]
public sealed class ZoorceValkyTagHelper : TagHelper
{
    /// <summary>Gets or sets the initial animation: idle, thinking, talking or celebrating.</summary>
    public string State { get; set; } = "idle";
    /// <summary>Gets or sets the accessible name of the companion.</summary>
    public string Label { get; set; } = "Valky, help assistant";
    /// <summary>Gets or sets an optional plain-text message when no child content is provided.</summary>
    public string? Message { get; set; }
    /// <summary>Gets or sets whether the bubble is initially open.</summary>
    public bool Open { get; set; }
    /// <summary>Gets or sets whether the companion is fixed to the viewport.</summary>
    public bool Floating { get; set; }
    /// <summary>Gets or sets a custom image URL. Custom art must use the same layer layout.</summary>
    [HtmlAttributeName("image-url")]
    public string? ImageUrl { get; set; }
    /// <summary>Gets or sets extra root classes.</summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }
    /// <summary>Gets or sets the current view context.</summary>
    [ViewContext, HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);
        var state = State?.Trim().ToLowerInvariant() ?? "idle";
        if (state is not ("idle" or "thinking" or "talking" or "celebrating"))
            throw new ArgumentException("Valky state must be idle, thinking, talking or celebrating.", nameof(State));

        var child = await output.GetChildContentAsync();
        var image = ImageUrl ?? $"{ViewContext?.HttpContext.Request.PathBase}/_content/Zoorce.Web.UI/images/valky.png";
        var encodedImage = HtmlEncoder.Default.Encode(image);
        var clip = "valky-" + Guid.NewGuid().ToString("N");
        var content = new HtmlContentBuilder();
        content.AppendHtml("<summary class=\"z-valky-trigger\" aria-label=\"");
        content.Append(Label);
        content.AppendHtml("\"><svg class=\"z-valky-art\" viewBox=\"0 0 1230 1278\" aria-hidden=\"true\" focusable=\"false\"><defs>");
        content.AppendHtml($"<clipPath id=\"{clip}-body\"><path d=\"M0 600 L305 600 L370 610 L470 650 L610 670 L740 540 L1230 540 L1230 1278 L0 1278Z\"/></clipPath><clipPath id=\"{clip}-head\"><path d=\"M0 0 H850 V440 L750 510 L680 625 L600 660 L470 640 L330 590 L280 550 H0Z\"/></clipPath><clipPath id=\"{clip}-tail\"><path d=\"M850 625 H1230 V1200 H990 L900 900Z\"/></clipPath></defs>");
        content.AppendHtml($"<image class=\"z-valky-body\" href=\"{encodedImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-body)\"/><image class=\"z-valky-tail\" href=\"{encodedImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-tail)\"/><g class=\"z-valky-head\"><image href=\"{encodedImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-head)\"/><g class=\"z-valky-blink\" fill=\"#262120\"><ellipse cx=\"440\" cy=\"415\" rx=\"42\" ry=\"48\"/><ellipse cx=\"600\" cy=\"330\" rx=\"37\" ry=\"47\"/></g></g></svg></summary>");
        content.AppendHtml("<div class=\"z-valky-bubble\" role=\"region\" aria-label=\"");
        content.Append(Label);
        content.AppendHtml("\">");
        if (!child.IsEmptyOrWhiteSpace) content.AppendHtml(child);
        else if (!string.IsNullOrWhiteSpace(Message)) content.Append(Message);
        content.AppendHtml("</div>");

        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("state"); output.Attributes.RemoveAll("label");
        output.Attributes.RemoveAll("message"); output.Attributes.RemoveAll("floating");
        output.Attributes.RemoveAll("image-url"); output.Attributes.RemoveAll("css-class");
        output.Attributes.SetAttribute("class", $"z-valky{(Floating ? " z-valky--floating" : "")} {CssClass}".Trim());
        output.Attributes.SetAttribute("data-z-valky-state", state);
        if (Open) output.Attributes.SetAttribute("open", "open");
        else output.Attributes.RemoveAll("open");
        output.Content.SetHtmlContent(content);
    }
}
