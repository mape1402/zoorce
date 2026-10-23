using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Renders an accessible accordion with independently owned disclosure state.</summary>
public sealed class ZoorceAccordionViewComponent : ViewComponent
{
    /// <summary>Renders the supplied accordion configuration.</summary>
    public IViewComponentResult Invoke(ZoorceAccordionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Id)) throw new ArgumentException("An accordion requires an id.", nameof(options));
        return View(options with { Id = options.Id.Trim(), Items = ZoorceNavigationNormalizers.AccordionItems(options.Items) });
    }
}
