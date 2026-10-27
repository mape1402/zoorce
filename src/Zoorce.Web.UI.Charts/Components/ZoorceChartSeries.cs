namespace Zoorce.Web.UI.Charts;

/// <summary>Represents one named series displayed by a Zoorce chart.</summary>
public sealed record ZoorceChartSeries(string Name, IReadOnlyList<ZoorceChartPoint> Points, string? Color = null)
{
    /// <summary>Gets the representation used by this series in a combo chart.</summary>
    public ZoorceChartSeriesKind RenderAs { get; init; } = ZoorceChartSeriesKind.Auto;

    /// <summary>Gets the value axis used by this series in a combo chart.</summary>
    public ZoorceChartAxis Axis { get; init; } = ZoorceChartAxis.Primary;
}
