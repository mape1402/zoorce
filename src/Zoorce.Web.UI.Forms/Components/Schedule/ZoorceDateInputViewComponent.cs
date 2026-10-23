using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated date input.</summary>
public sealed class ZoorceDateInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied date-input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDateInputOptions options) => Render(options);
}
