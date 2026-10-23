using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Foundation.Skeleton;

/// <summary>
/// Renders an accessible skeleton placeholder without prescribing a parent layout.
/// </summary>
public sealed class ZoorceSkeletonViewComponent : ViewComponent
{
    /// <summary>Renders a normalized skeleton placeholder.</summary>
    public IViewComponentResult Invoke(ZoorceSkeletonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return View(options.Normalize());
    }
}
