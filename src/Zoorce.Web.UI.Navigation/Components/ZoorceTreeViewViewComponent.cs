using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders a navigable tree view with independently owned expansion state.</summary>
public sealed class ZoorceTreeViewViewComponent : ViewComponent
{
    /// <summary>Renders the supplied tree view configuration.</summary>
    public IViewComponentResult Invoke(ZoorceTreeViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Label)) throw new ArgumentException("A tree view requires an id and label.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Label = options.Label.Trim(), Items = ZoorceNavigationNormalizers.TreeNodes(options.Items) });
    }
}
