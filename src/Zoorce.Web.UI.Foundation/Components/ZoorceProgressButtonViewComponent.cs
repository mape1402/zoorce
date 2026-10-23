using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a button whose fill layer reports host-controlled progress.</summary>
public sealed class ZoorceProgressButtonViewComponent : ViewComponent
{
    /// <summary>Renders the supplied progress button configuration.</summary>
    public IViewComponentResult Invoke(ZoorceProgressButtonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
