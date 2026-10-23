using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated color picker.</summary>
public sealed class ZoorceColorPickerViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied color-picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceColorPickerOptions options) => Render(options);
}
