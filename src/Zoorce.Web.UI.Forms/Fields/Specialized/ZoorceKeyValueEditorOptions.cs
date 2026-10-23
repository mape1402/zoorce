namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a key/value editor.</summary>
public sealed record ZoorceKeyValueEditorOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.KeyValue };
}
