namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a searchable multiple-selection dropdown.</summary>
public sealed record ZoorceMultiSelectDropdownOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets the text shown when no item is selected.</summary>
    public string PlaceholderText { get; init; } = "Choose options";

    /// <summary>Gets whether a built-in query box is rendered in the popover.</summary>
    public bool Searchable { get; init; } = true;

    /// <summary>Gets whether a select-all action is rendered.</summary>
    public bool ShowSelectAll { get; init; } = true;

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.MultiSelectDropdown, Multiple = true };
}
