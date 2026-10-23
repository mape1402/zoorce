namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an autocomplete combobox.</summary>
public sealed record ZoorceComboboxOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets whether the option list opens when the text entry receives focus.</summary>
    public bool OpenOnFocus { get; init; } = true;

    /// <summary>Gets the message shown when the entered query matches no configured choice.</summary>
    public string EmptyResultsText { get; init; } = "No matching options.";

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with
        {
            Kind = ZoorceFormControlKind.Combobox,
            OpenOnFocus = OpenOnFocus,
            EmptyResultsText = EmptyResultsText
        };
}
