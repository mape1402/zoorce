namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a selectable listbox with optional multiple selection.</summary>
public sealed record ZoorceListBoxOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets whether more than one choice can be selected.</summary>
    public bool Multiple { get; init; }

    /// <summary>Gets the maximum visible listbox height in pixels.</summary>
    public int MaxHeight { get; init; } = 240;

    /// <summary>Gets copy shown when the list contains no entries.</summary>
    public string EmptyText { get; init; } = "No options available.";

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.ListBox, Multiple = Multiple };
}
