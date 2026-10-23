namespace Zoorce.Web.UI.Abstractions.Navigation;

/// <summary>
/// Supplies web-shell navigation for the current request.
/// </summary>
public interface INavigationProvider
{
    /// <summary>
    /// Gets the items that should be displayed in the sidebar.
    /// </summary>
    IReadOnlyList<NavigationItem> GetItems();
}
