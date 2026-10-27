namespace Zoorce.Web.UI.Charts;

/// <summary>Configures pointer and keyboard data feedback.</summary>
public sealed record ZoorceChartTooltipOptions
{
    /// <summary>Gets whether tooltips are available.</summary>
    public bool Visible { get; init; } = true;
}
