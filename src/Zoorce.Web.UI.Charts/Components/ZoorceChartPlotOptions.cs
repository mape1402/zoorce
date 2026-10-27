namespace Zoorce.Web.UI.Charts;

/// <summary>Configures the optional visual details inside a chart plot.</summary>
public sealed record ZoorceChartPlotOptions
{
    /// <summary>Gets whether line-like series draw their visible outline.</summary>
    public bool ShowSeriesLines { get; init; } = true;

    /// <summary>Gets whether line-like series draw their terminal marker.</summary>
    public bool ShowLineMarkers { get; init; } = true;

    /// <summary>Gets whether chart-specific text annotations are rendered.</summary>
    public bool ShowDataLabels { get; init; } = true;

    /// <summary>Gets whether optional target markers are rendered.</summary>
    public bool ShowTargetMarker { get; init; } = true;

    /// <summary>Gets an optional viewport height in pixels; null selects the configured chart size.</summary>
    public int? Height { get; init; }

    /// <summary>Returns a plot configuration bounded to values that retain usable geometry.</summary>
    public ZoorceChartPlotOptions Normalize() => this with
    {
        Height = Height is null ? null : Math.Clamp(Height.Value, 120, 1_200)
    };
}
