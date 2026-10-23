using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zoorce.Web.UI.Abstractions.Navigation;

namespace Zoorce.Web.UI.Layout.Components;

/// <summary>
/// Renders the shell navigation from a host-provided navigation provider.
/// </summary>
public sealed class ZoorceSidebarNavigationViewComponent : ViewComponent
{
    private readonly ZoorceShellOptions _shell;
    private readonly INavigationProvider _navigationProvider;

    /// <summary>Initializes the component with shell and navigation services.</summary>
    public ZoorceSidebarNavigationViewComponent(IOptions<ZoorceShellOptions> shell, INavigationProvider navigationProvider)
    {
        _shell = shell?.Value ?? throw new ArgumentNullException(nameof(shell));
        _navigationProvider = navigationProvider ?? throw new ArgumentNullException(nameof(navigationProvider));
    }

    /// <summary>Renders the complete accessible sidebar navigation.</summary>
    public IViewComponentResult Invoke()
        => View(new ZoorceSidebarNavigationModel(_shell, _navigationProvider.GetItems()));
}

/// <summary>Supplies shell and navigation data to the sidebar component.</summary>
public sealed record ZoorceSidebarNavigationModel(
    ZoorceShellOptions Shell,
    IReadOnlyList<NavigationItem> Items);
