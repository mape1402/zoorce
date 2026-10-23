using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated toggle switch.</summary>
public sealed class ZoorceToggleSwitchViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied toggle configuration.</summary>
    public IViewComponentResult Invoke(ZoorceToggleSwitchOptions options) => Render(options);
}
