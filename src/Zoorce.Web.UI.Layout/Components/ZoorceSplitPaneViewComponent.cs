using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Layout;

/// <summary>Renders a keyboard-operable, user-resizable split pane.</summary>
public sealed class ZoorceSplitPaneViewComponent : ViewComponent
{
    /// <summary>Renders the supplied split-pane configuration.</summary>
    public IViewComponentResult Invoke(ZoorceSplitPaneOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
