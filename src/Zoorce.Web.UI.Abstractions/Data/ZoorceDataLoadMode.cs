namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Selects how a data component presents successive result pages.
/// </summary>
public enum ZoorceDataLoadMode
{
    /// <summary>Replaces the current rows through explicit pagination controls.</summary>
    Pagination,

    /// <summary>Appends the next result page as the visitor reaches the end of the list.</summary>
    InfiniteScroll
}
