using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated autocomplete combobox.</summary>
public sealed class ZoorceComboboxViewComponent : ViewComponent
{
    /// <summary>Renders the supplied combobox configuration.</summary>
    public IViewComponentResult Invoke(ZoorceComboboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.ToRenderOptions());
    }
}
