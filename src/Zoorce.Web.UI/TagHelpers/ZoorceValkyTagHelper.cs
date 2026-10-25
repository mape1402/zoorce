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
    /// <summary>Gets or sets the head artwork URL. Custom artwork must use the same layer layout.</summary>
    [HtmlAttributeName("image-url")]
    public string? ImageUrl { get; set; }
    /// <summary>Gets or sets the headless body artwork URL, aligned with the head artwork.</summary>
    [HtmlAttributeName("body-image-url")]
    public string? BodyImageUrl { get; set; }
    /// <summary>Gets or sets whether celebration repeats until the host changes the state.</summary>
    [HtmlAttributeName("celebration-loop")]
    public bool CelebrationLoop { get; set; }
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
        var bodyImage = HtmlEncoder.Default.Encode(BodyImageUrl ?? $"{ViewContext?.HttpContext.Request.PathBase}/_content/Zoorce.Web.UI/images/valky-body.png");
        var script = HtmlEncoder.Default.Encode($"{ViewContext?.HttpContext.Request.PathBase}/_content/Zoorce.Web.UI/js/zoorce-valky.js");
        var clip = "valky-" + Guid.NewGuid().ToString("N");
        var content = new HtmlContentBuilder();
        content.AppendHtml("<summary class=\"z-valky-trigger\" aria-label=\"");
        content.Append(Label);
        content.AppendHtml("\"><svg class=\"z-valky-art\" viewBox=\"0 0 1230 1278\" aria-hidden=\"true\" focusable=\"false\"><defs>");
        content.AppendHtml($"<clipPath id=\"{clip}-body\"><path d=\"M0 0 H840 L850 650 L885 740 L930 835 L985 1030 L1000 1278 H0Z\"/></clipPath><clipPath id=\"{clip}-head\"><path d=\"M0 0 H840 V425 L750 450 L716 500 L680 548 L644 592 L610 624 L558 635 L500 625 L445 608 L392 585 L340 567 L300 550 L270 520 H0Z\"/></clipPath><clipPath id=\"{clip}-tail\"><path d=\"M840 620 H1230 V1200 H950 L885 850Z\"/></clipPath></defs>");
        content.AppendHtml($"<image class=\"z-valky-tail\" href=\"{bodyImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-tail)\"/><image class=\"z-valky-body\" href=\"{bodyImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-body)\"/><g class=\"z-valky-head\"><image href=\"{encodedImage}\" width=\"1230\" height=\"1278\" clip-path=\"url(#{clip}-head)\"/><g class=\"z-valky-blink\" fill=\"#292421\"><ellipse cx=\"443\" cy=\"416\" rx=\"43\" ry=\"40\"/><ellipse cx=\"601\" cy=\"333\" rx=\"39\" ry=\"42\"/></g><path class=\"z-valky-mouth-cover\" d=\"M525 527 Q557 507 581 513 Q615 494 649 499 Q657 510 650 533 Q637 581 599 593 Q558 604 536 568 Z\" fill=\"#241e1d\"/></g></svg></summary>");
        content.AppendHtml("<div class=\"z-valky-bubble\" role=\"region\" aria-label=\"");
        content.Append(Label);
        content.AppendHtml("\">");
        if (!child.IsEmptyOrWhiteSpace) content.AppendHtml(child);
        else if (!string.IsNullOrWhiteSpace(Message)) content.Append(Message);
        content.AppendHtml("</div>");
        content.AppendHtml($"<script type=\"module\" src=\"{script}\"></script>");

        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("state"); output.Attributes.RemoveAll("label");
        output.Attributes.RemoveAll("message"); output.Attributes.RemoveAll("floating");
        output.Attributes.RemoveAll("image-url"); output.Attributes.RemoveAll("body-image-url"); output.Attributes.RemoveAll("celebration-loop"); output.Attributes.RemoveAll("css-class");
        output.Attributes.SetAttribute("class", $"z-valky{(Floating ? " z-valky--floating" : "")} {CssClass}".Trim());
        output.Attributes.SetAttribute("data-z-valky-state", state);
        output.Attributes.SetAttribute("data-z-valky-celebration", CelebrationLoop ? "loop" : "once");
        if (Open) output.Attributes.SetAttribute("open", "open");
        else output.Attributes.RemoveAll("open");
        output.Content.SetHtmlContent(content);
    }
}
