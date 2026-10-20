using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Components.DataGrid;

/// <summary>Configures an isolated DataGrid instance and its progressive data transport.</summary>
public sealed record ZoorceDataGridOptions
{
    /// <summary>Gets the unique DOM identifier for the grid instance.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the accessible description of the grid.</summary>
    public string AriaLabel { get; init; } = "Data grid";

    /// <summary>Gets the REST endpoint that returns the <c>ZoorceDataPage</c> JSON envelope.</summary>
    public string SourceUrl { get; init; } = string.Empty;

    /// <summary>Gets the optional SignalR hub endpoint used for live invalidation or deltas.</summary>
    public string? RealtimeHubUrl { get; init; }

    /// <summary>Gets the SignalR event name listened to by this grid.</summary>
    public string? RealtimeEventName { get; init; }

    /// <summary>Gets the presentation used for successive data pages.</summary>
    public ZoorceDataLoadMode LoadMode { get; init; } = ZoorceDataLoadMode.Pagination;

    /// <summary>Gets the number of records requested from the data source.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets whether the component renders a local search field.</summary>
    public bool EnableSearch { get; init; } = true;

    /// <summary>Gets the configured table columns.</summary>
    public IReadOnlyList<ZoorceDataGridColumn> Columns { get; init; } = [];

    /// <summary>Gets server-rendered rows used before JavaScript enhances the component.</summary>
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> InitialItems { get; init; } = [];

    /// <summary>Gets the total count associated with <see cref="InitialItems"/> when it is known.</summary>
    public int? InitialTotalCount { get; init; }

    /// <summary>Gets whether an additional virtual-window hint should be applied while appending rows.</summary>
    public bool Virtualize { get; init; }

    /// <summary>Gets the field that uniquely identifies a row for stable updates and virtual scrolling.</summary>
    public string RowKeyField { get; init; } = "id";

    /// <summary>Gets the fixed height, in pixels, of the scroll viewport used by infinite mode.</summary>
    public int ViewportHeight { get; init; } = 520;

    /// <summary>Gets the distance from the end of the viewport that requests the next page.</summary>
    public int PrefetchThreshold { get; init; } = 144;

    /// <summary>Gets the number of additional virtual rows preserved before and after the visible window.</summary>
    public int VirtualizationOverscan { get; init; } = 8;

    /// <summary>Gets how this isolated grid handles optional realtime changes.</summary>
    public ZoorceDataGridRealtimeBehavior RealtimeBehavior { get; init; } = ZoorceDataGridRealtimeBehavior.Badge;

    /// <summary>Validates and normalizes host-provided configuration.</summary>
    public ZoorceDataGridOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A data grid requires a unique id.", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(SourceUrl))
        {
            throw new ArgumentException("A data grid requires a REST source URL.", nameof(SourceUrl));
        }

        if (Columns.Count == 0)
        {
            throw new ArgumentException("A data grid requires at least one column.", nameof(Columns));
        }

        return this with
        {
            Id = Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Data grid" : AriaLabel.Trim(),
            SourceUrl = SourceUrl.Trim(),
            RealtimeHubUrl = string.IsNullOrWhiteSpace(RealtimeHubUrl) ? null : RealtimeHubUrl.Trim(),
            RealtimeEventName = string.IsNullOrWhiteSpace(RealtimeEventName) ? null : RealtimeEventName.Trim(),
            PageSize = Math.Clamp(PageSize, 1, 100),
            RowKeyField = string.IsNullOrWhiteSpace(RowKeyField) ? "id" : RowKeyField.Trim(),
            ViewportHeight = Math.Clamp(ViewportHeight, 240, 1_200),
            PrefetchThreshold = Math.Clamp(PrefetchThreshold, 0, 1_000),
            VirtualizationOverscan = Math.Clamp(VirtualizationOverscan, 0, 40)
        };
    }
}
