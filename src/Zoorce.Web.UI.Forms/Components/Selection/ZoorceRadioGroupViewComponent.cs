using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated radio group.</summary>
public sealed class ZoorceRadioGroupViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied radio-group configuration.</summary>
    public IViewComponentResult Invoke(ZoorceRadioGroupOptions options) => Render(options);
}
