using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated tag-input field.</summary>
public sealed class ZoorceTagInputViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied tag-input configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTagInputOptions options) => Render(options);
}
