using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.DataGrid;

/// <summary>Renders a transport-independent, progressively enhanced data grid.</summary>
public sealed class ZoorceDataGridViewComponent : ViewComponent
{
    /// <summary>Renders the configured grid with accessible server-side fallback rows.</summary>
    public IViewComponentResult Invoke(ZoorceDataGridOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
