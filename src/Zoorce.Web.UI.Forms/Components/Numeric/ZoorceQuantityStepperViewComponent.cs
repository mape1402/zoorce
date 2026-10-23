using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an encapsulated quantity stepper.</summary>
public sealed class ZoorceQuantityStepperViewComponent : ZoorceFieldViewComponent
{
    /// <summary>Renders the supplied quantity-stepper configuration.</summary>
    public IViewComponentResult Invoke(ZoorceQuantityStepperOptions options) => Render(options);
}
