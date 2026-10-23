namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a segmented single-value selector.</summary>
public sealed record ZoorceSegmentedControlOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.SegmentedControl };
}
