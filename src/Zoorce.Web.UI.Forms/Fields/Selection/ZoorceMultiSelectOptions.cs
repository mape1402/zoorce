namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a multiple-value select field.</summary>
public sealed record ZoorceMultiSelectOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.MultiSelect };
}
