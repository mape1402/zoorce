namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a mutually exclusive radio group.</summary>
public sealed record ZoorceRadioGroupOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.RadioGroup };
}
