using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders an accessible inline message.</summary>
public sealed class ZoorceMessageViewComponent : ViewComponent
{
    /// <summary>Renders the supplied message configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMessageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Title)) throw new ArgumentException("A message requires an id and title.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Title = options.Title.Trim(), Tone = string.IsNullOrWhiteSpace(options.Tone) ? "info" : options.Tone.Trim().ToLowerInvariant() });
    }
}
