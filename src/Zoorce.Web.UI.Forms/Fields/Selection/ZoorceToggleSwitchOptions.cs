namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a binary toggle switch.</summary>
public sealed record ZoorceToggleSwitchOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.ToggleSwitch };
}
