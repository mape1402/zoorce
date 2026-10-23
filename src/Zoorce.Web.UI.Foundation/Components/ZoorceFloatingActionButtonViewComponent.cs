using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a reusable floating action button.</summary>
public sealed class ZoorceFloatingActionButtonViewComponent : ViewComponent
{
    /// <summary>Renders the supplied floating action configuration.</summary>
    public IViewComponentResult Invoke(ZoorceFloatingActionButtonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Label) || string.IsNullOrWhiteSpace(options.Icon)) throw new ArgumentException("A floating action button requires an id, label, and icon.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Label = options.Label.Trim(), Icon = options.Icon.Trim(), Tone = string.IsNullOrWhiteSpace(options.Tone) ? "primary" : options.Tone.Trim().ToLowerInvariant() });
    }
}
