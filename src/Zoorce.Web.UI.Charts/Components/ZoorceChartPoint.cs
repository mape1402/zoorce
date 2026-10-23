namespace Zoorce.Web.UI.Charts;

/// <summary>
/// Represents one labeled numeric observation. Optional dimensions support continuous,
/// ranged and matrix-based visualizations without requiring a second component contract.
/// </summary>
public sealed record ZoorceChartPoint(
    string Label,
    decimal Value,
    decimal? X = null,
    decimal? Low = null,
    decimal? High = null,
    decimal? Size = null,
    string? Group = null);
