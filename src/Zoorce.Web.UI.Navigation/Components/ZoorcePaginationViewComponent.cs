using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders an accessible paginator with optional page-size and direct navigation controls.</summary>
public sealed class ZoorcePaginationViewComponent : ViewComponent
{
    /// <summary>Renders the supplied paginator configuration.</summary>
    public IViewComponentResult Invoke(ZoorcePaginationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
