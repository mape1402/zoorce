using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated hidden input.</summary>
public sealed class ZoorceHiddenInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied hidden input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceHiddenInputOptions options) => Render(options);
}
