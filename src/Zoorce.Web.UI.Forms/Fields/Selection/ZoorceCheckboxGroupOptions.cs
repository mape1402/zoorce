namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a multiple-value checkbox group.</summary>
public sealed record ZoorceCheckboxGroupOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.CheckboxGroup };
}
