using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Renders a generic dialog without coupling it to a specific host workflow.</summary>
public sealed class ZoorceDialogViewComponent : ViewComponent
{
    /// <summary>Renders the supplied dialog configuration.</summary>
    public IViewComponentResult Invoke(ZoorceDialogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Title)) throw new ArgumentException("A dialog requires an id and title.", nameof(options));
        var size = options.Size?.Trim().ToLowerInvariant();
        size = size is "compact" or "default" or "wide" or "full" ? size : "default";
        return View(options with { Id = options.Id.Trim(), Title = options.Title.Trim(), Size = size });
    }
}
