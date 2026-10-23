namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a compact numeric up/down field.</summary>
public sealed record ZoorceNumericInputOptions : ZoorceNumericFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.NumericUpDown };
}
