namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a semantic hidden input.</summary>
public sealed record ZoorceHiddenInputOptions : ZoorceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Hidden };
}
