using Microsoft.AspNetCore.Mvc;
using Zoorce.Web.UI.Abstractions.Navigation;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>
/// Renders the reusable command palette from host navigation items.
/// </summary>
public sealed class ZoorceCommandPaletteViewComponent : ViewComponent
{
    private readonly INavigationProvider _navigationProvider;

    /// <summary>Initializes the component with the host navigation provider.</summary>
    public ZoorceCommandPaletteViewComponent(INavigationProvider navigationProvider)
        => _navigationProvider = navigationProvider ?? throw new ArgumentNullException(nameof(navigationProvider));

    /// <summary>Renders every navigable leaf item as a palette action.</summary>
    public IViewComponentResult Invoke() => View(GetLeafItems(_navigationProvider.GetItems()));

    private static IReadOnlyList<NavigationItem> GetLeafItems(IEnumerable<NavigationItem> items)
        => items.SelectMany(item => item.HasChildren ? GetLeafItems(item.Children!) : [item])
            .Where(item => !string.IsNullOrWhiteSpace(item.Url))
            .ToArray();
}
