using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a visual avatar independent from file-upload behavior.</summary>
public sealed class ZoorceAvatarViewComponent : ViewComponent
{
    /// <summary>Renders the supplied avatar configuration.</summary>
    public IViewComponentResult Invoke(ZoorceAvatarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Label)) throw new ArgumentException("An avatar requires a label.", nameof(options));
        var label = options.Label.Trim();
        var initials = string.IsNullOrWhiteSpace(options.Initials)
            ? string.Concat(label.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => part[0])).ToUpperInvariant()
            : options.Initials.Trim()[..Math.Min(3, options.Initials.Trim().Length)].ToUpperInvariant();
        return View(options with { Label = label, Initials = initials, Size = Math.Clamp(options.Size, 20, 160), Status = string.IsNullOrWhiteSpace(options.Status) ? null : options.Status.Trim().ToLowerInvariant() });
    }
}
