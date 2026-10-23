using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated icon picker.</summary>
public sealed class ZoorceIconPickerViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied icon-picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceIconPickerOptions options) => Render(options);
}
