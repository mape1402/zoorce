using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zoerce.WebUI.Options;

namespace Zoerce.WebUI.Components.Diagrams;

/// <summary>Renders an isolated diagram surface with a useful server-side fallback.</summary>
public sealed class ZoorceDiagramViewComponent : ViewComponent
{
    private readonly ZoorceDiagramExportOptions _exportDefaults;

    /// <summary>Creates the component using the export defaults registered for the UI package.</summary>
    public ZoorceDiagramViewComponent(IOptions<ZoerceWebUiOptions>? options = null)
    {
        _exportDefaults = options?.Value.DiagramExport ?? new ZoorceDiagramExportOptions();
    }

    /// <summary>Renders a normalized diagram configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDiagramOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize(_exportDefaults));
    }
}
