namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an icon picker.</summary>
public sealed record ZoorceIconPickerOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.IconPicker };
}
