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
        if (options.HeaderMode == ZoorceDialogHeaderMode.None && options.Header is not null) throw new ArgumentException("A hidden dialog header cannot include custom header content.", nameof(options));
        var size = options.Size?.Trim().ToLowerInvariant();
        size = size is "compact" or "default" or "wide" or "full" ? size : "default";
        return View(options with
        {
            Id = options.Id.Trim(),
            Title = options.Title.Trim(),
            Eyebrow = TrimOrNull(options.Eyebrow),
            Description = TrimOrNull(options.Description),
            HeaderMode = Enum.IsDefined(options.HeaderMode) ? options.HeaderMode : ZoorceDialogHeaderMode.Standard,
            BodyPadding = Enum.IsDefined(options.BodyPadding) ? options.BodyPadding : ZoorceDialogBodyPadding.Default,
            CssClass = NormalizeCssClasses(options.CssClass),
            BodyCssClass = NormalizeCssClasses(options.BodyCssClass),
            FooterCssClass = NormalizeCssClasses(options.FooterCssClass),
            Size = size,
            FixedHeight = TrimOrNull(options.FixedHeight)
        });
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCssClasses(string? value) => string.IsNullOrWhiteSpace(value)
        ? null
        : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
