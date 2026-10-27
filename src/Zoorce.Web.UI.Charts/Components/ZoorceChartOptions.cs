using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Charts;

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

    /// <summary>Gets presentation settings. A bare surface leaves the host in control of the card.</summary>
    public ZoorceChartAppearanceOptions Appearance { get; init; } = new();

    /// <summary>Gets settings for the title, summary and current metric area.</summary>
    public ZoorceChartHeaderOptions Header { get; init; } = new();

    /// <summary>Gets settings for the optional series legend.</summary>
    public ZoorceChartLegendOptions Legend { get; init; } = new();

    /// <summary>Gets settings for pointer and keyboard data feedback.</summary>
    public ZoorceChartTooltipOptions Tooltip { get; init; } = new();

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

    /// <summary>Gets the hole ratio for a donut chart. Set this to zero to render a pie chart.</summary>
    public decimal DonutInnerRadius { get; init; } = .56m;

    /// <summary>Gets an optional suffix presented with the primary metric, for example <c>%</c>.</summary>
    public string? ValueSuffix { get; init; }

    /// <summary>Gets the transport-agnostic data binding used to refresh chart series.</summary>
    public ZoorceDataBindingOptions Binding { get; init; } = new();

    /// <summary>Gets initial server-rendered series.</summary>
    public IReadOnlyList<ZoorceChartSeries> Series { get; init; } = [];

    /// <summary>Gets financial observations for a candlestick chart.</summary>
    public IReadOnlyList<ZoorceChartOhlcPoint> OhlcPoints { get; init; } = [];

    /// <summary>Gets statistical observations for a box plot.</summary>
    public IReadOnlyList<ZoorceChartBoxPlotPoint> BoxPlotPoints { get; init; } = [];

    /// <summary>Gets weighted rectangles for a treemap.</summary>
    public IReadOnlyList<ZoorceChartTreemapItem> TreemapItems { get; init; } = [];

    /// <summary>Gets nodes for a Sankey diagram.</summary>
    public IReadOnlyList<ZoorceChartSankeyNode> SankeyNodes { get; init; } = [];

    /// <summary>Gets relationships for a Sankey diagram.</summary>
    public IReadOnlyList<ZoorceChartSankeyLink> SankeyLinks { get; init; } = [];

    /// <summary>Gets scheduled items for a Gantt chart.</summary>
    public IReadOnlyList<ZoorceChartGanttItem> GanttItems { get; init; } = [];

    /// <summary>Validates and normalizes the component configuration.</summary>
    public ZoorceChartOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A chart requires a unique id.", nameof(Id));
        }

        var binding = (Binding ?? new ZoorceDataBindingOptions()).Normalize();
        if (RequiresSpecializedData(Kind) && string.IsNullOrWhiteSpace(binding.Source) && !HasSpecializedData())
        {
            throw new ArgumentException($"A {Kind} chart requires its corresponding specialized data.", nameof(Kind));
        }

        if (!RequiresSpecializedData(Kind) && Series.Count == 0 && string.IsNullOrWhiteSpace(binding.Source))
        {
            throw new ArgumentException("A chart requires initial series or a REST source URL.", nameof(Series));
        }

        if (MinimumValue is not null && MaximumValue is not null && MinimumValue > MaximumValue)
        {
            throw new ArgumentException("The minimum chart value cannot exceed the maximum chart value.", nameof(MinimumValue));
        }

        if (DonutInnerRadius is < 0 or >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(DonutInnerRadius), "The donut inner radius must be greater than or equal to zero and less than one.");
        }

        return this with
        {
            Id = Id.Trim(),
            Title = string.IsNullOrWhiteSpace(Title) ? "Chart" : Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(Summary) ? "Data visualization" : Summary.Trim(),
            ValueSuffix = string.IsNullOrWhiteSpace(ValueSuffix) ? null : ValueSuffix.Trim(),
            Binding = binding,
            Appearance = (Appearance ?? new ZoorceChartAppearanceOptions()) with { CssClass = TrimOrNull(Appearance?.CssClass) },
            Header = Header ?? new ZoorceChartHeaderOptions(),
            Legend = Legend ?? new ZoorceChartLegendOptions(),
            Tooltip = Tooltip ?? new ZoorceChartTooltipOptions()
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool RequiresSpecializedData(ZoorceChartKind kind) => kind is ZoorceChartKind.Candlestick or ZoorceChartKind.BoxPlot or ZoorceChartKind.Treemap or ZoorceChartKind.Sankey or ZoorceChartKind.Gantt;

    private bool HasSpecializedData() => Kind switch
    {
        ZoorceChartKind.Candlestick => OhlcPoints.Count > 0,
        ZoorceChartKind.BoxPlot => BoxPlotPoints.Count > 0,
        ZoorceChartKind.Treemap => TreemapItems.Count > 0,
        ZoorceChartKind.Sankey => SankeyNodes.Count > 0 && SankeyLinks.Count > 0,
        ZoorceChartKind.Gantt => GanttItems.Count > 0,
        _ => false
    };

}
