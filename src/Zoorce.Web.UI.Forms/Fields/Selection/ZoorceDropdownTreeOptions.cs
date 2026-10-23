namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a dropdown that selects from a hierarchical tree.</summary>
public sealed record ZoorceDropdownTreeOptions : ZoorceFieldOptions
{
    /// <summary>Gets the root nodes rendered by the dropdown.</summary>
    public IReadOnlyList<ZoorceTreeChoice> Items { get; init; } = [];

    /// <summary>Gets whether multiple nodes can be selected.</summary>
    public bool Multiple { get; init; }

    /// <summary>Gets whether all branches start expanded.</summary>
    public bool ExpandAll { get; init; }

    /// <summary>Gets placeholder copy shown before a node is selected.</summary>
    public string PlaceholderText { get; init; } = "Choose a node";

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.DropdownTree, Multiple = Multiple };
}
