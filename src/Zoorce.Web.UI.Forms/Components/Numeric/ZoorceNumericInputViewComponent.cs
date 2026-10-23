using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated numeric up/down input.</summary>
public sealed class ZoorceNumericInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied numeric input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceNumericInputOptions options) => Render(options);
}
