namespace Zoorce.Web.UI.Charts;

/// <summary>Configures the optional title and summary area.</summary>
public sealed record ZoorceChartHeaderOptions
{
    /// <summary>Gets whether the title and summary are rendered.</summary>
    public bool Visible { get; init; } = true;

    /// <summary>Gets whether the current metric and delta are rendered.</summary>
    public bool ShowMetric { get; init; } = true;
}
