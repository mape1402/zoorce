using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated range slider.</summary>
public sealed class ZoorceRangeSliderViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied range-slider configuration.</summary>
    public IViewComponentResult Invoke(ZoorceRangeSliderOptions options) => Render(options);
}
