using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated masked input.</summary>
public sealed class ZoorceMaskedInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied masked input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMaskedInputOptions options) => Render(options);
}
