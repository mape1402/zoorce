using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders a contextual menu without owning the host target's markup.</summary>
public sealed class ZoorceContextMenuViewComponent : ViewComponent
{
    /// <summary>Renders the supplied context menu configuration.</summary>
    public IViewComponentResult Invoke(ZoorceContextMenuOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.TargetSelector)) throw new ArgumentException("A context menu requires an id and target selector.", nameof(options));
        return View(options with { Id = options.Id.Trim(), TargetSelector = options.TargetSelector.Trim(), Items = ZoorceNavigationNormalizers.MenuItems(options.Items) });
    }
}
