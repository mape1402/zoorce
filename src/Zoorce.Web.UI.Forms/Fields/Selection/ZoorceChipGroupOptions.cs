namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a selectable chip group.</summary>
public sealed record ZoorceChipGroupOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.ChipGroup };
}
