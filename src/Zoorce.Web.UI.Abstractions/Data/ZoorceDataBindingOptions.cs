namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Identifies a transport-agnostic data binding owned by one rendered component instance.
/// </summary>
public sealed record ZoorceDataBindingOptions
{
    /// <summary>Gets the host-registered source key or a URI resolved by the browser data runtime.</summary>
    public string? Source { get; init; }

    /// <summary>Gets the invalidation scope that refreshes only this component instance.</summary>
    public string? InvalidationScope { get; init; }

    /// <summary>Gets loading behavior; it is enabled by default for data-bound controls.</summary>
    public ZoorceLoadingOptions Loading { get; init; } = new();

    /// <summary>Gets the action requested when this binding receives an invalidation.</summary>
    public ZoorceDataRefreshStrategy RefreshStrategy { get; init; } = ZoorceDataRefreshStrategy.Refresh;

    /// <summary>Normalizes optional values without forcing a particular transport.</summary>
    public ZoorceDataBindingOptions Normalize()
        => this with
        {
            Source = TrimOrNull(Source),
            InvalidationScope = TrimOrNull(InvalidationScope),
            Loading = (Loading ?? new ZoorceLoadingOptions()).Normalize(),
            RefreshStrategy = Enum.IsDefined(RefreshStrategy) ? RefreshStrategy : ZoorceDataRefreshStrategy.Refresh
        };

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
