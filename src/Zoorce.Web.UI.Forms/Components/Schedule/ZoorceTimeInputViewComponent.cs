using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated time input.</summary>
public sealed class ZoorceTimeInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied time-input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTimeInputOptions options) => Render(options);
}
