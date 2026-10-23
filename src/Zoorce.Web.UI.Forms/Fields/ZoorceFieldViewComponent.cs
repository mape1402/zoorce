using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>
/// Provides the shared rendering boundary for named, encapsulated form fields.
/// </summary>
public abstract class ZoorceFieldViewComponent : ViewComponent
{
    /// <summary>Renders one normalized field through the internal field template.</summary>
    protected IViewComponentResult Render(ZoorceFieldOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View("~/Views/Shared/Components/ZoorceFormControl/Default.cshtml", options.ToRenderOptions());
    }
}
