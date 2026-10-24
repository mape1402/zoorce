using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders an encapsulated drawer with modal focus behavior.</summary>
public sealed class ZoorceDrawerViewComponent : ViewComponent
{
    /// <summary>Renders the supplied drawer configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDrawerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
