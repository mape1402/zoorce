using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated chip group.</summary>
public sealed class ZoorceChipGroupViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied chip-group configuration.</summary>
    public IViewComponentResult Invoke(ZoorceChipGroupOptions options) => Render(options);
}
