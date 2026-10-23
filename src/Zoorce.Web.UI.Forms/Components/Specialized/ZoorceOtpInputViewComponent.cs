using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated verification-code input.</summary>
public sealed class ZoorceOtpInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied verification-code configuration.</summary>
    public IViewComponentResult Invoke(ZoorceOtpInputOptions options) => Render(options);
}
