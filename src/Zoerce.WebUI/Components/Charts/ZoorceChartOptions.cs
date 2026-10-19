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

        return this with
        {
            Id = Id.Trim(),
            Title = string.IsNullOrWhiteSpace(Title) ? "Chart" : Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(Summary) ? "Data visualization" : Summary.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(SourceUrl) ? null : SourceUrl.Trim(),
            RealtimeHubUrl = string.IsNullOrWhiteSpace(RealtimeHubUrl) ? null : RealtimeHubUrl.Trim(),
            RealtimeEventName = string.IsNullOrWhiteSpace(RealtimeEventName) ? null : RealtimeEventName.Trim()
        };
    }
}
