using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders a semantic breadcrumb trail that can collapse middle locations.</summary>
public sealed class ZoorceBreadcrumbViewComponent : ViewComponent
{
    /// <summary>Renders the supplied breadcrumb configuration.</summary>
    public IViewComponentResult Invoke(ZoorceBreadcrumbOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
