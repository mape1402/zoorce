using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>
/// Renders an independent tooltip surface that any host trigger can reference with <c>aria-describedby</c>.
/// </summary>
public sealed class ZoorceTooltipViewComponent : ViewComponent
{
    /// <summary>Renders a normalized passive tooltip.</summary>
    public IViewComponentResult Invoke(ZoorceTooltipOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
