using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures an accessible, focus-managed drawer surface.</summary>
public sealed record ZoorceDrawerOptions
{
    /// <summary>Gets the unique DOM identifier for the drawer.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible and visible drawer title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets optional supporting copy displayed below the title.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the edge from which the drawer opens.</summary>
    public ZoorceDrawerPosition Position { get; init; } = ZoorceDrawerPosition.End;

    /// <summary>Gets the standard size applied to the drawer surface.</summary>
    public ZoorceDrawerSize Size { get; init; } = ZoorceDrawerSize.Default;

    /// <summary>Gets an optional fixed CSS size that overrides the standard size token.</summary>
    public string? FixedSize { get; init; }

    /// <summary>Gets whether a visible close button is rendered.</summary>
    public bool ShowCloseButton { get; init; } = true;

    /// <summary>Gets whether pressing the backdrop closes the drawer.</summary>
    public bool CloseOnBackdrop { get; init; } = true;

    /// <summary>Gets body content rendered between the header and footer.</summary>
    public IHtmlContent? Body { get; init; }

    /// <summary>Gets optional persistent footer content.</summary>
    public IHtmlContent? Footer { get; init; }

    /// <summary>Gets additional CSS classes applied to the drawer surface.</summary>
    public string? CssClass { get; init; }

    /// <summary>Validates and normalizes the drawer configuration.</summary>
    public ZoorceDrawerOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A drawer requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("A drawer requires a title.", nameof(Title));
        return this with
        {
            Id = Id.Trim(),
            Title = Title.Trim(),
            Description = TrimOrNull(Description),
            Position = Enum.IsDefined(Position) ? Position : ZoorceDrawerPosition.End,
            Size = Enum.IsDefined(Size) ? Size : ZoorceDrawerSize.Default,
            FixedSize = TrimOrNull(FixedSize),
            CssClass = NormalizeCssClasses(CssClass)
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCssClasses(string? value) => string.IsNullOrWhiteSpace(value)
        ? null
        : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
