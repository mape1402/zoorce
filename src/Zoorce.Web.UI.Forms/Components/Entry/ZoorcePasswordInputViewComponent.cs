using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated password input.</summary>
public sealed class ZoorcePasswordInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied password input configuration.</summary>
    public IViewComponentResult Invoke(ZoorcePasswordInputOptions options) => Render(options);
}
