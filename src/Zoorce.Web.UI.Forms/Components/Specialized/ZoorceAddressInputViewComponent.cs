using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated address input.</summary>
public sealed class ZoorceAddressInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied address-input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceAddressInputOptions options) => Render(options);
}
