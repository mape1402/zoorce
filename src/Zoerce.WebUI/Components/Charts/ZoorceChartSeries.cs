namespace Zoerce.WebUI.Components.Charts;

/// <summary>Represents one named series displayed by a Zoorce chart.</summary>
public sealed record ZoorceChartSeries(string Name, IReadOnlyList<ZoorceChartPoint> Points);
