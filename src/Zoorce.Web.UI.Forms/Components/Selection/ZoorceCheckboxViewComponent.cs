using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated checkbox.</summary>
public sealed class ZoorceCheckboxViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied checkbox configuration.</summary>
    public IViewComponentResult Invoke(ZoorceCheckboxOptions options) => Render(options);
}
