namespace Zoorce.Web.UI.Charts;

/// <summary>Configures the host-owned presentation around a chart.</summary>
public sealed record ZoorceChartAppearanceOptions
{
    /// <summary>Gets the surface used by the component.</summary>
    public ZoorceChartSurface Surface { get; init; } = ZoorceChartSurface.Card;

    /// <summary>Gets an optional CSS class applied to the chart root.</summary>
    public string? CssClass { get; init; }
}
