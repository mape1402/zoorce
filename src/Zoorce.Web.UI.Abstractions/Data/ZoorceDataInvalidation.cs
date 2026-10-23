namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Describes a transport-neutral request to refresh one named data scope.
/// </summary>
public sealed record ZoorceDataInvalidation
{
    /// <summary>Gets the name shared by components that should receive the update.</summary>
    public required string Scope { get; init; }

    /// <summary>Gets the preferred strategy for applying the update.</summary>
    public ZoorceDataRefreshStrategy Strategy { get; init; } = ZoorceDataRefreshStrategy.Refresh;

    /// <summary>Gets an optional item-level change supplied by a realtime transport.</summary>
    public object? Change { get; init; }

    /// <summary>Gets a host-defined correlation id used for diagnostics.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Returns a normalized invalidation message.</summary>
    public ZoorceDataInvalidation Normalize()
    {
        if (string.IsNullOrWhiteSpace(Scope))
        {
            throw new ArgumentException("A data invalidation requires a scope.", nameof(Scope));
        }

        return this with
        {
            Scope = Scope.Trim(),
            Strategy = Enum.IsDefined(Strategy) ? Strategy : ZoorceDataRefreshStrategy.Refresh,
            CorrelationId = string.IsNullOrWhiteSpace(CorrelationId) ? null : CorrelationId.Trim()
        };
    }
}
