namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Represents one location in a breadcrumb trail.</summary>
public sealed record ZoorceBreadcrumbItem
{
    /// <summary>Gets the label shown to the user.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the navigation target for non-current locations.</summary>
    public string? Href { get; init; }

    /// <summary>Gets an optional icon rendered before the label.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets whether this location represents the current page.</summary>
    public bool Current { get; init; }
}
