using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders an accessible tablist with keyboard selection.</summary>
public sealed class ZoorceTabsViewComponent : ViewComponent
{
    /// <summary>Renders the supplied tabs configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTabsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id)) throw new ArgumentException("A tab set requires an id.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Items = ZoorceNavigationNormalizers.Tabs(options.Items) });
    }
}
