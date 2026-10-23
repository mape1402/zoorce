namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Represents the complete observable state of one data-bound component instance.
/// </summary>
public sealed record ZoorceDataSnapshot<T>
{
    /// <summary>Gets the current lifecycle state.</summary>
    public ZoorceDataLoadState State { get; init; } = ZoorceDataLoadState.Idle;

    /// <summary>Gets the most recently accepted page of data.</summary>
    public ZoorceDataPage<T>? Page { get; init; }

    /// <summary>Gets the error from the latest failed request, when any.</summary>
    public Exception? Error { get; init; }

    /// <summary>Gets the optional known completion percentage.</summary>
    public double? Progress { get; init; }

    /// <summary>Gets the time at which the snapshot changed.</summary>
    public DateTimeOffset ChangedAt { get; init; } = DateTimeOffset.UtcNow;
}
