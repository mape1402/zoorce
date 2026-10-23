namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a time input.</summary>
public sealed record ZoorceTimeInputOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Time };
}
