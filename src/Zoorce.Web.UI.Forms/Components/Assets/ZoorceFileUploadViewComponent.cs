using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated file-upload control.</summary>
public sealed class ZoorceFileUploadViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied file-upload configuration.</summary>
    public IViewComponentResult Invoke(ZoorceFileUploadOptions options) => Render(options);
}
