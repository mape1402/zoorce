using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated date range input.</summary>
public sealed class ZoorceDateRangeViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied date-range configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDateRangeOptions options) => Render(options);
}
