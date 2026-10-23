using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Timelines;

/// <summary>Renders an accessible, transport-independent timeline with a server-side fallback.</summary>
public sealed class ZoorceTimelineViewComponent : ViewComponent
{
    /// <summary>Renders a normalized timeline configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTimelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
