using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated date-and-time input.</summary>
public sealed class ZoorceDateTimeInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied date-and-time configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDateTimeInputOptions options) => Render(options);
}
