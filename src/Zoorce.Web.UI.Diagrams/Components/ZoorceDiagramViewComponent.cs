using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Diagrams;

/// <summary>Renders an isolated diagram surface with a useful server-side fallback.</summary>
public sealed class ZoorceDiagramViewComponent : ViewComponent
{
    /// <summary>Renders a normalized diagram configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDiagramOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
