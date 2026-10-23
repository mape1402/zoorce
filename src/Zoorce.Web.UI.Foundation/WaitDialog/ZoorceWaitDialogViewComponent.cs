using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.WaitDialog;

/// <summary>
/// Renders an explicit wait dialog controlled by its owning host operation.
/// </summary>
public sealed class ZoorceWaitDialogViewComponent : ViewComponent
{
    /// <summary>Renders a normalized wait dialog.</summary>
    public IViewComponentResult Invoke(ZoorceWaitDialogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
