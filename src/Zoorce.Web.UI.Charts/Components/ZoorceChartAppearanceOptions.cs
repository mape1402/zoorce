namespace Zoorce.Web.UI.Charts;

/// <summary>Configures the host-owned presentation around a chart.</summary>
public sealed record ZoorceChartAppearanceOptions
{
    /// <summary>Gets the surface used by the component.</summary>
    public ZoorceChartSurface Surface { get; init; } = ZoorceChartSurface.Card;

    /// <summary>Gets whether the component surface paints its background.</summary>
    public bool ShowSurfaceBackground { get; init; } = true;

    /// <summary>Gets whether the component surface paints its border.</summary>
    public bool ShowSurfaceBorder { get; init; } = true;

    /// <summary>Gets whether the plot viewport paints its background.</summary>
    public bool ShowPlotBackground { get; init; } = true;

    /// <summary>Gets whether the plot viewport paints its border.</summary>
    public bool ShowPlotBorder { get; init; } = true;

    /// <summary>Gets an optional CSS class applied to the chart root.</summary>
    public string? CssClass { get; init; }
}
