using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated single-value select field.</summary>
public sealed class ZoorceSelectViewComponent : ViewComponent
{
    /// <summary>Renders the supplied select configuration.</summary>
    public IViewComponentResult Invoke(ZoorceSelectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.ToRenderOptions());
    }
}
