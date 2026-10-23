using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>
/// Renders the reusable accessible confirmation-dialog host.
/// </summary>
public sealed class ZoorceConfirmationDialogViewComponent : ViewComponent
{
    /// <summary>Renders the initially hidden dialog host.</summary>
    public IViewComponentResult Invoke() => View();
}
