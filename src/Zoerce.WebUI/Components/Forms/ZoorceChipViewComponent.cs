using Microsoft.AspNetCore.Mvc;

namespace Zoerce.WebUI.Components.Forms;

/// <summary>Renders an independent Zoorce chip.</summary>
public sealed class ZoorceChipViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ZoorceChipOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
