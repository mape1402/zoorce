namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a two-value range slider.</summary>
public sealed record ZoorceRangeSliderOptions : ZoorceNumericFieldOptions
{
    /// <summary>Gets the end value.</summary>
    public string? EndValue { get; init; }
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.RangeSlider, SecondaryValue = EndValue };
}
