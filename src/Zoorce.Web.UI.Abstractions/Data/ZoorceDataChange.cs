namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Describes a live data mutation emitted by a host-owned realtime transport.
/// </summary>
public sealed record ZoorceDataChange<T>(
    ZoorceDataChangeKind Kind,
    string Key,
    T? Item = default);

/// <summary>Identifies the operation represented by a live data change.</summary>
public enum ZoorceDataChangeKind
{
    /// <summary>Inserts or updates one item.</summary>
    Upsert,

    /// <summary>Removes the item identified by <see cref="ZoorceDataChange{T}.Key"/>.</summary>
    Remove,

    /// <summary>Requests a fresh query because the local result cannot be safely patched.</summary>
    Invalidate
}
