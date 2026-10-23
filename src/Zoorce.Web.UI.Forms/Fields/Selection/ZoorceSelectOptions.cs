namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a single-value select field.</summary>
public sealed record ZoorceSelectOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets whether the visitor can clear a selected value from the menu.</summary>
    public bool AllowClear { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.Select, AllowClear = AllowClear };
}
