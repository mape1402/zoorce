using Microsoft.AspNetCore.Mvc;
namespace Zoorce.Web.UI.Forms;
/// <summary>Renders an encapsulated segmented control.</summary>
public sealed class ZoorceSegmentedControlViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied segmented-control configuration.</summary>
    public IViewComponentResult Invoke(ZoorceSegmentedControlOptions options) => Render(options);
}
