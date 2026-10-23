namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Represents one reusable action or navigation entry rendered by menu-like components.</summary>
public sealed record ZoorceMenuItem
{
    /// <summary>Gets the stable value emitted when the item is selected.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the visible item label.</summary>
    public required string Text { get; init; }

    /// <summary>Gets optional supporting copy rendered beneath the label.</summary>
    public string? Description { get; init; }

    /// <summary>Gets an optional icon or glyph rendered before the label.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets an optional navigation destination.</summary>
    public string? Href { get; init; }

    /// <summary>Gets an optional visible keyboard shortcut hint.</summary>
    public string? Shortcut { get; init; }

    /// <summary>Gets whether the item cannot be selected.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets nested child items for a submenu or hierarchical control.</summary>
    public IReadOnlyList<ZoorceMenuItem> Children { get; init; } = [];
}
