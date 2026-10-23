using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated image-upload control.</summary>
public sealed class ZoorceImageUploadViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied image-upload configuration.</summary>
    public IViewComponentResult Invoke(ZoorceImageUploadOptions options) => Render(options);
}
