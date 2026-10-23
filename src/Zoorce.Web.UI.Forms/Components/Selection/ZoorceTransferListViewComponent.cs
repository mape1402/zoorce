using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated transfer list.</summary>
public sealed class ZoorceTransferListViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied transfer-list configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTransferListOptions options) => Render(options);
}
