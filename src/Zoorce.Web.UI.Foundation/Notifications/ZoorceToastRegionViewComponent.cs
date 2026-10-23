using Microsoft.AspNetCore.Mvc;
using Zoorce.Web.UI.Abstractions.Notifications;

namespace Zoorce.Web.UI.Foundation.Notifications;

/// <summary>
/// Renders pending toast messages from the scoped toast service.
/// </summary>
public sealed class ZoorceToastRegionViewComponent : ViewComponent
{
    private readonly IToastService _toasts;

    /// <summary>Initializes the component with the host toast service.</summary>
    public ZoorceToastRegionViewComponent(IToastService toasts)
        => _toasts = toasts ?? throw new ArgumentNullException(nameof(toasts));

    /// <summary>Renders and drains queued messages for the current response.</summary>
    public IViewComponentResult Invoke() => View(_toasts.Drain());
}
