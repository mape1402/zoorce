using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated rating selector.</summary>
public sealed class ZoorceRatingViewComponent : ViewComponent
{
    /// <summary>Renders the supplied rating configuration.</summary>
    public IViewComponentResult Invoke(ZoorceRatingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.ToRenderOptions());
    }
}
