using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated emoji picker.</summary>
public sealed class ZoorceEmojiPickerViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied emoji-picker configuration.</summary>
    public IViewComponentResult Invoke(ZoorceEmojiPickerOptions options) => Render(options);
}
