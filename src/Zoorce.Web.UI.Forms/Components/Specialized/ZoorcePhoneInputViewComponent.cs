using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated phone input.</summary>
public sealed class ZoorcePhoneInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied phone-input configuration.</summary>
    public IViewComponentResult Invoke(ZoorcePhoneInputOptions options) => Render(options);
}
