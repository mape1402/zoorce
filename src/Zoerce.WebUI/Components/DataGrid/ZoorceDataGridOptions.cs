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
            PageSize = Math.Clamp(PageSize, 1, 100)
        };
    }
}
