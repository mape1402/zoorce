namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a date-and-time input.</summary>
public sealed record ZoorceDateTimeInputOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.DateTime };
}
