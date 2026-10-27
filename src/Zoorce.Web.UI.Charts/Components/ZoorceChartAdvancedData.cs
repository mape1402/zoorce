namespace Zoorce.Web.UI.Charts;

/// <summary>Represents a financial observation for a candlestick chart.</summary>
public sealed record ZoorceChartOhlcPoint(string Label, decimal Open, decimal High, decimal Low, decimal Close, DateTimeOffset? Timestamp = null);

/// <summary>Represents the five-number summary used by a box plot.</summary>
public sealed record ZoorceChartBoxPlotPoint(
    string Label,
    decimal Minimum,
    decimal FirstQuartile,
    decimal Median,
    decimal ThirdQuartile,
    decimal Maximum,
    IReadOnlyList<decimal>? Outliers = null);

/// <summary>Represents one weighted item in a treemap.</summary>
public sealed record ZoorceChartTreemapItem(string Id, string Label, decimal Value, string? Group = null, string? Color = null);

/// <summary>Represents a named node in a Sankey diagram.</summary>
public sealed record ZoorceChartSankeyNode(string Id, string Label, string? Color = null);

/// <summary>Represents a weighted directional relationship in a Sankey diagram.</summary>
public sealed record ZoorceChartSankeyLink(string SourceId, string TargetId, decimal Value, string? Label = null, string? Color = null);

/// <summary>Represents one scheduled item in a Gantt chart.</summary>
public sealed record ZoorceChartGanttItem(
    string Id,
    string Label,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Group = null,
    decimal Progress = 0,
    string? Color = null);
