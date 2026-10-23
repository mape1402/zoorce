using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated currency input.</summary>
public sealed class ZoorceCurrencyInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied currency input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceCurrencyInputOptions options) => Render(options);
}
