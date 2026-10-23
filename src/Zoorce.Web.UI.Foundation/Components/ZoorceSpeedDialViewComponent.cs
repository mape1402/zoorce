using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders an encapsulated speed dial for compact contextual actions.</summary>
public sealed class ZoorceSpeedDialViewComponent : ViewComponent
{
    /// <summary>Renders the supplied speed dial configuration.</summary>
    public IViewComponentResult Invoke(ZoorceSpeedDialOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Label)) throw new ArgumentException("A speed dial requires an id and label.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Label = options.Label.Trim(), Icon = string.IsNullOrWhiteSpace(options.Icon) ? "+" : options.Icon.Trim() });
    }
}
