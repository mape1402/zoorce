namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a selectable hierarchical tree view.</summary>
public sealed record ZoorceTreeViewOptions
{
    /// <summary>Gets the unique DOM identifier for the tree.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible tree label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets root nodes in display order.</summary>
    public IReadOnlyList<ZoorceTreeNode> Items { get; init; } = [];

    /// <summary>Gets whether multiple nodes can be selected.</summary>
    public bool Multiple { get; init; }
}
