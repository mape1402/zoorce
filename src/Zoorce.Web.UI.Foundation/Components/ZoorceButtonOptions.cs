namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>
/// Configures an accessible reusable button or link action.
/// </summary>
public sealed record ZoorceButtonOptions
{
    /// <summary>Gets the visible button text.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the semantic visual style.</summary>
    public string Tone { get; init; } = "primary";

    /// <summary>Gets the optional URL that renders this action as a link.</summary>
    public string? Href { get; init; }

    /// <summary>Gets the optional leading icon or glyph.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets the optional DOM identifier.</summary>
    public string? Id { get; init; }

    /// <summary>Gets an optional accessible label.</summary>
    public string? AriaLabel { get; init; }

    /// <summary>Gets whether the action is disabled.</summary>
    public bool Disabled { get; init; }

    /// <summary>Gets whether the compact button density is used.</summary>
    public bool Compact { get; init; }

    /// <summary>Normalizes optional presentation values.</summary>
    public ZoorceButtonOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            throw new ArgumentException("A button requires visible text.", nameof(Text));
        }

        return this with
        {
            Text = Text.Trim(),
            Tone = string.IsNullOrWhiteSpace(Tone) ? "primary" : Tone.Trim().ToLowerInvariant(),
            Href = string.IsNullOrWhiteSpace(Href) ? null : Href.Trim(),
            Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim(),
            Id = string.IsNullOrWhiteSpace(Id) ? null : Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? null : AriaLabel.Trim()
        };
    }
}
