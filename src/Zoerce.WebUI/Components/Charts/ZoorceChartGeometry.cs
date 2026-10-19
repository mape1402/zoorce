namespace Zoerce.WebUI.Components.Charts;

/// <summary>Provides deterministic coordinate calculations for native SVG line charts.</summary>
public static class ZoorceChartGeometry
{
    /// <summary>Builds normalized coordinates in a 0-100 plotting area.</summary>
    public static IReadOnlyList<(decimal X, decimal Y)> Normalize(IReadOnlyList<ZoorceChartPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count == 0)
        {
            return [];
        }

        var minimum = points.Min(point => point.Value);
        var maximum = points.Max(point => point.Value);
        var range = maximum - minimum;
        var divisor = Math.Max(points.Count - 1, 1);

        return points.Select((point, index) =>
        {
            var x = index * 100m / divisor;
            var y = range == 0 ? 50m : 100m - ((point.Value - minimum) * 100m / range);
            return (x, y);
        }).ToArray();
    }
}
