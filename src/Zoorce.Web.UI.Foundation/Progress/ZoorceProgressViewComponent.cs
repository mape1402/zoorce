using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Progress;

/// <summary>
/// Renders an accessible progress indicator that can be updated by an owning component.
/// </summary>
public sealed class ZoorceProgressViewComponent : ViewComponent
{
    /// <summary>Renders a normalized progress indicator.</summary>
    public IViewComponentResult Invoke(ZoorceProgressOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
