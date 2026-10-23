using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders an action toolbar that wraps safely when space is constrained.</summary>
public sealed class ZoorceToolbarViewComponent : ViewComponent
{
    /// <summary>Renders the supplied toolbar configuration.</summary>
    public IViewComponentResult Invoke(ZoorceToolbarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id) || string.IsNullOrWhiteSpace(options.Label)) throw new ArgumentException("A toolbar requires an id and label.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Label = options.Label.Trim(), Items = ZoorceNavigationNormalizers.MenuItems(options.Items) });
    }
}
