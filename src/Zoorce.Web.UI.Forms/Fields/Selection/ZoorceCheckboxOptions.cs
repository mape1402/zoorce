namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a boolean checkbox.</summary>
public sealed record ZoorceCheckboxOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Checkbox };
}
