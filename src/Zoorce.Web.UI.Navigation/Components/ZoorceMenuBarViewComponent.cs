using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders a keyboard-navigable horizontal menu bar.</summary>
public sealed class ZoorceMenuBarViewComponent : ViewComponent
{
    /// <summary>Renders the supplied menu bar configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMenuBarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id)) throw new ArgumentException("A menu bar requires an id.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Items = ZoorceNavigationNormalizers.MenuItems(options.Items) });
    }
}
