using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Represents one selectable tab and its independently supplied panel content.</summary>
public sealed record ZoorceTabItem
{
    /// <summary>Gets the stable tab identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the tab label.</summary>
    public required string Text { get; init; }

    /// <summary>Gets optional tab icon.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets optional compact badge copy.</summary>
    public string? Badge { get; init; }

    /// <summary>Gets panel content supplied by the host.</summary>
    public IHtmlContent? Content { get; init; }

    /// <summary>Gets whether the tab starts selected.</summary>
    public bool Selected { get; init; }

    /// <summary>Gets whether the tab is disabled.</summary>
    public bool Disabled { get; init; }
}
