using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated duration input.</summary>
public sealed class ZoorceDurationInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied duration configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDurationInputOptions options) => Render(options);
}
