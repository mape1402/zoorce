namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a single-value range slider.</summary>
public sealed record ZoorceSliderOptions : ZoorceNumericFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.Slider };
}
