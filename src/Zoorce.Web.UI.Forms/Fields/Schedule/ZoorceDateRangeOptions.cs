namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a two-value date range input.</summary>
public sealed record ZoorceDateRangeOptions : ZoorceFieldOptions
{
    /// <summary>Gets the final date value.</summary>
    public string? EndValue { get; init; }
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.DateRange, SecondaryValue = EndValue };
}
