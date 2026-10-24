using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a keyboard-accessible calendar date picker.</summary>
public sealed class ZoorceDatePickerViewComponent : ViewComponent
{
    /// <summary>Renders the supplied date picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDatePickerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
