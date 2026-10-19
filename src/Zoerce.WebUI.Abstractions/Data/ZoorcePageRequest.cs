namespace Zoerce.WebUI.Abstractions.Data;

/// <summary>
/// Requests one data window through a page number or an opaque cursor.
/// </summary>
public sealed record ZoorcePageRequest
{
    /// <summary>Gets the one-based page number used by offset-backed providers.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets the requested number of records.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Gets an optional opaque continuation cursor.</summary>
    public string? Cursor { get; init; }

    /// <summary>Gets the zero-based offset for providers that do not use cursors.</summary>
    public int Offset => checked((Page - 1) * PageSize);

    /// <summary>
    /// Normalizes invalid requests before they reach a data source.
    /// </summary>
    public ZoorcePageRequest Normalize(int maximumPageSize = 100)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumPageSize, 1);

        return this with
        {
            Page = Math.Max(Page, 1),
            PageSize = Math.Clamp(PageSize, 1, maximumPageSize),
            Cursor = string.IsNullOrWhiteSpace(Cursor) ? null : Cursor
        };
    }
}
