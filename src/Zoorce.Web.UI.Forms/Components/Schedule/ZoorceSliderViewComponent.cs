using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated slider.</summary>
public sealed class ZoorceSliderViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied slider configuration.</summary>
    public IViewComponentResult Invoke(ZoorceSliderOptions options) => Render(options);
}
