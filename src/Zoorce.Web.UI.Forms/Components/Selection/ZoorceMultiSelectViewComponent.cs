using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated multi-select field.</summary>
public sealed class ZoorceMultiSelectViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied multi-select configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMultiSelectOptions options) => Render(options);
}
