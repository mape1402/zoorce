using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an independent Zoorce chip.</summary>
public sealed class ZoorceChipViewComponent : ViewComponent
{
    /// <summary>Represents the <c>Invoke</c> option.</summary>
    public IViewComponentResult Invoke(ZoorceChipOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
