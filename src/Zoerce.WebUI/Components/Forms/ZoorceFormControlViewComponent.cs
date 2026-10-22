using Microsoft.AspNetCore.Mvc;

namespace Zoerce.WebUI.Components.Forms;

/// <summary>Renders one isolated, progressively enhanced form control.</summary>
public sealed class ZoorceFormControlViewComponent : ViewComponent
{
    /// <summary>Renders a normalized form control.</summary>
    public IViewComponentResult Invoke(ZoorceFormControlOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
