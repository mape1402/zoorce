using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated checkbox group.</summary>
public sealed class ZoorceCheckboxGroupViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied checkbox-group configuration.</summary>
    public IViewComponentResult Invoke(ZoorceCheckboxGroupOptions options) => Render(options);
}
