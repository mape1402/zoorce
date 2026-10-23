using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Charts;

/// <summary>Renders an accessible native-SVG chart with optional client-side live enhancement.</summary>
public sealed class ZoorceChartViewComponent : ViewComponent
{
    /// <summary>Renders the chart and its server-side data summary.</summary>
    public IViewComponentResult Invoke(ZoorceChartOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
