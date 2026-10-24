using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures an accessible generic dialog surface.</summary>
public sealed record ZoorceDialogOptions
{
    /// <summary>Gets the unique DOM identifier for the dialog.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the dialog title, including its accessible name when the visual header is hidden.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the optional eyebrow shown by the standard header.</summary>
    public string? Eyebrow { get; init; } = "Dialog";

    /// <summary>Gets optional supporting copy.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the built-in header treatment.</summary>
    public ZoorceDialogHeaderMode HeaderMode { get; init; } = ZoorceDialogHeaderMode.Standard;

    /// <summary>Gets optional custom header content that replaces the generated title treatment.</summary>
    public IHtmlContent? Header { get; init; }

    /// <summary>Gets whether a close control is rendered.</summary>
    public bool ShowCloseButton { get; init; } = true;

    /// <summary>Gets custom dialog body content.</summary>
    public IHtmlContent? Body { get; init; }

    /// <summary>Gets the amount of spacing applied around the body slot.</summary>
    public ZoorceDialogBodyPadding BodyPadding { get; init; } = ZoorceDialogBodyPadding.Default;

    /// <summary>Gets additional CSS classes applied to the dialog surface.</summary>
    public string? CssClass { get; init; }

    /// <summary>Gets additional CSS classes applied to the dialog body slot.</summary>
    public string? BodyCssClass { get; init; }

    /// <summary>Gets custom dialog footer content.</summary>
    public IHtmlContent? Footer { get; init; }

    /// <summary>Gets additional CSS classes applied to the dialog footer slot.</summary>
    public string? FooterCssClass { get; init; }

    /// <summary>Gets the maximum dialog width token: compact, default, wide, or full.</summary>
    public string Size { get; init; } = "default";

    /// <summary>Gets the optional fixed surface height. The body becomes scrollable when its content exceeds this height.</summary>
    public string? FixedHeight { get; init; }

    /// <summary>Gets whether clicking the backdrop closes the dialog.</summary>
    public bool CloseOnBackdrop { get; init; } = true;
}
