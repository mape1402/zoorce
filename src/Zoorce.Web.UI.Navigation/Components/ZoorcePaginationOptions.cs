namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a page navigator for a finite collection.</summary>
public sealed record ZoorcePaginationOptions
{
    /// <summary>Gets the unique DOM identifier for the paginator.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the current one-based page.</summary>
    public int CurrentPage { get; init; } = 1;

    /// <summary>Gets the total number of available pages.</summary>
    public int TotalPages { get; init; } = 1;

    /// <summary>Gets the current collection page size.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Gets selectable page sizes.</summary>
    public IReadOnlyList<int> PageSizes { get; init; } = [10, 25, 50, 100];

    /// <summary>Gets whether a page-size selector is rendered.</summary>
    public bool ShowPageSize { get; init; } = true;

    /// <summary>Gets whether a direct page jump input is rendered.</summary>
    public bool ShowJumpToPage { get; init; }

    /// <summary>Gets whether the host owns state changes instead of the component.</summary>
    public bool IsControlled { get; init; }

    /// <summary>Gets the number of numbered page buttons rendered around the current page.</summary>
    public int WindowSize { get; init; } = 5;

    /// <summary>Gets the accessible label announced for the paginator.</summary>
    public string AriaLabel { get; init; } = "Pagination";

    /// <summary>Validates and normalizes the paginator configuration.</summary>
    public ZoorcePaginationOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A paginator requires an id.", nameof(Id));
        var totalPages = Math.Max(1, TotalPages);
        var pageSizes = PageSizes.Where(size => size > 0).Distinct().Order().ToArray();
        if (pageSizes.Length == 0) pageSizes = [Math.Max(1, PageSize)];
        var pageSize = Math.Max(1, PageSize);
        if (!pageSizes.Contains(pageSize)) pageSizes = pageSizes.Append(pageSize).Order().ToArray();
        return this with
        {
            Id = Id.Trim(),
            TotalPages = totalPages,
            CurrentPage = Math.Clamp(CurrentPage, 1, totalPages),
            PageSize = pageSize,
            PageSizes = pageSizes,
            WindowSize = Math.Clamp(WindowSize, 3, 11),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Pagination" : AriaLabel.Trim()
        };
    }
}
