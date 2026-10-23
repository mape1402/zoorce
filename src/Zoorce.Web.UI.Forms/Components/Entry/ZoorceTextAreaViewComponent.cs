using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated multiline text input.</summary>
public sealed class ZoorceTextAreaViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied text area configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTextAreaOptions options) => Render(options);
}
