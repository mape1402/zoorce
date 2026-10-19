namespace Zoerce.WebUI.Abstractions.Data;

/// <summary>
/// Represents one result page returned by a server-side or REST data source.
/// </summary>
public sealed record ZoorceDataPage<T>(
    IReadOnlyList<T> Items,
    int? TotalCount = null,
    bool HasMore = false,
    string? NextCursor = null)
{
    /// <summary>Creates an empty page with no continuation.</summary>
    public static ZoorceDataPage<T> Empty() => new([]);
}
