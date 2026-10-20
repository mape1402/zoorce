namespace Zoerce.WebUI.Components.Charts;

/// <summary>Configures an isolated, native-SVG chart component.</summary>
public sealed record ZoorceChartOptions
{
    /// <summary>Gets the unique component identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the visible chart title.</summary>
    public string Title { get; init; } = "Chart";

    /// <summary>Gets the accessible chart summary.</summary>
    public string Summary { get; init; } = "Data visualization";

    /// <summary>Gets the visualization type.</summary>
    public ZoorceChartKind Kind { get; init; } = ZoorceChartKind.Line;

    /// <summary>Gets the responsive visual density of the chart.</summary>
    public ZoorceChartSize Size { get; init; } = ZoorceChartSize.Standard;

    /// <summary>Gets how bar series should be composed when the visualization is a bar chart.</summary>
    public ZoorceChartBarMode BarMode { get; init; } = ZoorceChartBarMode.Grouped;

    /// <summary>Gets the interpolation used by line-like visualizations.</summary>
    public ZoorceChartCurve Curve { get; init; } = ZoorceChartCurve.Smooth;

    /// <summary>Gets whether the chart legend is visible and can toggle a series.</summary>
    public bool ShowLegend { get; init; } = true;

    /// <summary>Gets whether plot grid lines are rendered.</summary>
    public bool ShowGrid { get; init; } = true;

    /// <summary>Gets whether axes and their labels are rendered where applicable.</summary>
    public bool ShowAxes { get; init; } = true;

    /// <summary>Gets whether pointer and keyboard tooltips are enabled.</summary>
    public bool ShowTooltip { get; init; } = true;

    /// <summary>Gets the optional lower bound for bounded or continuous charts.</summary>
    public decimal? MinimumValue { get; init; }

    /// <summary>Gets the optional upper bound for bounded or continuous charts.</summary>
    public decimal? MaximumValue { get; init; }

    /// <summary>Gets an optional goal marker used by bullet and gauge charts.</summary>
    public decimal? TargetValue { get; init; }

    /// <summary>Gets an optional suffix presented with the primary metric, for example <c>%</c>.</summary>
    public string? ValueSuffix { get; init; }

    /// <summary>Gets an optional REST source that returns chart series.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Gets an optional SignalR hub endpoint used for live redraws.</summary>
    public string? RealtimeHubUrl { get; init; }

    /// <summary>Gets the SignalR event name listened to by this chart.</summary>
    public string? RealtimeEventName { get; init; }

    /// <summary>Gets initial server-rendered series.</summary>
    public IReadOnlyList<ZoorceChartSeries> Series { get; init; } = [];

    /// <summary>Validates and normalizes the component configuration.</summary>
    public ZoorceChartOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A chart requires a unique id.", nameof(Id));
        }

        if (Series.Count == 0 && string.IsNullOrWhiteSpace(SourceUrl))
        {
            throw new ArgumentException("A chart requires initial series or a REST source URL.", nameof(Series));
        }

        if (MinimumValue is not null && MaximumValue is not null && MinimumValue > MaximumValue)
        {
            throw new ArgumentException("The minimum chart value cannot exceed the maximum chart value.", nameof(MinimumValue));
        }

        return this with
        {
            Id = Id.Trim(),
            Title = string.IsNullOrWhiteSpace(Title) ? "Chart" : Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(Summary) ? "Data visualization" : Summary.Trim(),
            ValueSuffix = string.IsNullOrWhiteSpace(ValueSuffix) ? null : ValueSuffix.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(SourceUrl) ? null : SourceUrl.Trim(),
            RealtimeHubUrl = string.IsNullOrWhiteSpace(RealtimeHubUrl) ? null : RealtimeHubUrl.Trim(),
            RealtimeEventName = string.IsNullOrWhiteSpace(RealtimeEventName) ? null : RealtimeEventName.Trim()
        };
    }
}
