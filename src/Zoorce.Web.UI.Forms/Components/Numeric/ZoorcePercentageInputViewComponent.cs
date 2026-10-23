using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated percentage input.</summary>
public sealed class ZoorcePercentageInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied percentage input configuration.</summary>
    public IViewComponentResult Invoke(ZoorcePercentageInputOptions options) => Render(options);
}
