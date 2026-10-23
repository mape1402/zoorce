using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Stepper;

/// <summary>Renders an accessible, transport-independent stepper with a server-side fallback.</summary>
public sealed class ZoorceStepperViewComponent : ViewComponent
{
    /// <summary>Renders a normalized stepper configuration.</summary>
    public IViewComponentResult Invoke(ZoorceStepperOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
