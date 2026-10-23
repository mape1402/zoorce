namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a date input.</summary>
public sealed record ZoorceDateInputOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Date };
}
