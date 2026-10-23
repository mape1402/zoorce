using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a reusable content card.</summary>
public sealed class ZoorceCardViewComponent : ViewComponent
{
    /// <summary>Renders the supplied card configuration.</summary>
    public IViewComponentResult Invoke(ZoorceCardOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id)) throw new ArgumentException("A card requires an id.", nameof(options));
        return View(options with { Id = options.Id.Trim() });
    }
}
