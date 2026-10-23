using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>
/// Renders one reusable Zoorce button or link action.
/// </summary>
public sealed class ZoorceButtonViewComponent : ViewComponent
{
    /// <summary>Renders a normalized button configuration.</summary>
    public IViewComponentResult Invoke(ZoorceButtonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
