using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated text input.</summary>
public sealed class ZoorceTextInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied text input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTextInputOptions options) => Render(options);
}
