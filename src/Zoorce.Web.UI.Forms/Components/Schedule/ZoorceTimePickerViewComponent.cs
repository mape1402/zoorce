using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a keyboard-accessible, themeable time picker.</summary>
public sealed class ZoorceTimePickerViewComponent : ViewComponent
{
    /// <summary>Renders the supplied time-picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTimePickerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
