using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders an encapsulated dropdown menu with keyboard support.</summary>
public sealed class ZoorceDropdownMenuViewComponent : ViewComponent
{
    /// <summary>Renders the supplied menu configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDropdownMenuOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
