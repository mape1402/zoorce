using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures an accessible generic dialog surface.</summary>
public sealed record ZoorceDialogOptions
{
    /// <summary>Gets the unique DOM identifier for the dialog.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the visible dialog title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets optional supporting copy.</summary>
    public string? Description { get; init; }

    /// <summary>Gets custom dialog body content.</summary>
    public IHtmlContent? Body { get; init; }

    /// <summary>Gets custom dialog footer content.</summary>
    public IHtmlContent? Footer { get; init; }

    /// <summary>Gets the maximum dialog width token: compact, default, wide, or full.</summary>
    public string Size { get; init; } = "default";

    /// <summary>Gets whether clicking the backdrop closes the dialog.</summary>
    public bool CloseOnBackdrop { get; init; } = true;
}
