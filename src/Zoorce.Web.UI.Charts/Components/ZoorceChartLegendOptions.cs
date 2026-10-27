namespace Zoorce.Web.UI.Charts;

/// <summary>Configures the optional, interactive series legend.</summary>
public sealed record ZoorceChartLegendOptions
{
    /// <summary>Gets whether the legend is visible.</summary>
    public bool Visible { get; init; } = true;

    /// <summary>Gets whether legend buttons can toggle a series.</summary>
    public bool Interactive { get; init; } = true;
}
