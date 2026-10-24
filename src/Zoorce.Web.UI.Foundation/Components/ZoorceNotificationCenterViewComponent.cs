using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders an encapsulated notification trigger and panel.</summary>
public sealed class ZoorceNotificationCenterViewComponent : ViewComponent
{
    /// <summary>Renders the supplied notification-center configuration.</summary>
    public IViewComponentResult Invoke(ZoorceNotificationCenterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
