namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Represents a tree node rendered by <see cref="ZoorceTreeViewViewComponent"/>.</summary>
public sealed record ZoorceTreeNode
{
    /// <summary>Gets the stable node value.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the visible node label.</summary>
    public required string Text { get; init; }

    /// <summary>Gets an optional node icon.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets child nodes in display order.</summary>
    public IReadOnlyList<ZoorceTreeNode> Children { get; init; } = [];

    /// <summary>Gets whether the node begins expanded.</summary>
    public bool Expanded { get; init; }

    /// <summary>Gets whether the node begins selected.</summary>
    public bool Selected { get; init; }

    /// <summary>Gets whether the node cannot be selected.</summary>
    public bool Disabled { get; init; }
}
