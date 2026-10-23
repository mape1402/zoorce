using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a reusable content card with independently supplied regions.</summary>
public sealed record ZoorceCardOptions
{
    /// <summary>Gets the unique DOM identifier for the card.</summary>
    public required string Id { get; init; }

    /// <summary>Gets optional card title text.</summary>
    public string? Title { get; init; }

    /// <summary>Gets optional card description text.</summary>
    public string? Description { get; init; }

    /// <summary>Gets optional custom body content.</summary>
    public IHtmlContent? Body { get; init; }

    /// <summary>Gets optional custom footer content.</summary>
    public IHtmlContent? Footer { get; init; }

    /// <summary>Gets optional image URL rendered above the card content.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Gets whether the card is exposed as a clickable surface.</summary>
    public bool Interactive { get; init; }
}
