using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a reusable badge or status dot.</summary>
public sealed class ZoorceBadgeViewComponent : ViewComponent
{
    /// <summary>Renders the supplied badge configuration.</summary>
    public IViewComponentResult Invoke(ZoorceBadgeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Dot && string.IsNullOrWhiteSpace(options.GetDisplayValue())) throw new ArgumentException("A badge requires text or a count unless it is a dot.", nameof(options));
        if (options.Dot && string.IsNullOrWhiteSpace(options.AriaLabel)) throw new ArgumentException("A dot badge requires an accessible label.", nameof(options));
        return View(options with { Tone = string.IsNullOrWhiteSpace(options.Tone) ? "primary" : options.Tone.Trim().ToLowerInvariant(), MaximumCount = Math.Max(1, options.MaximumCount) });
    }
}
