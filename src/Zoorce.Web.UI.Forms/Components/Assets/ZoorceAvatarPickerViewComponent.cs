using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated avatar picker.</summary>
public sealed class ZoorceAvatarPickerViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied avatar-picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceAvatarPickerOptions options) => Render(options);
}
