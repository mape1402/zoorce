using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated key/value editor.</summary>
public sealed class ZoorceKeyValueEditorViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied key/value editor configuration.</summary>
    public IViewComponentResult Invoke(ZoorceKeyValueEditorOptions options) => Render(options);
}
