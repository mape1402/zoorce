namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a presentational or removable chip.</summary>
public sealed record ZoorceChipOptions
{
    /// <summary>Represents the <c>Text</c> option.</summary>
    public string Text { get; init; } = string.Empty;
    /// <summary>Represents the <c>Icon</c> option.</summary>
    public string? Icon { get; init; }
    /// <summary>Represents the <c>Tone</c> option.</summary>
    public string? Tone { get; init; }
    /// <summary>Represents the <c>Removable</c> option.</summary>
    public bool Removable { get; init; }
    /// <summary>Represents the <c>Selected</c> option.</summary>
    public bool Selected { get; init; }
    /// <summary>Represents the <c>Disabled</c> option.</summary>
    public bool Disabled { get; init; }
    /// <summary>Represents the <c>AriaLabel</c> option.</summary>
    public string? AriaLabel { get; init; }

    /// <summary>Represents the <c>Normalize</c> option.</summary>
    public ZoorceChipOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Text)) throw new ArgumentException("A chip requires text.", nameof(Text));
        return this with
        {
            Text = Text.Trim(),
            Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim(),
            Tone = string.IsNullOrWhiteSpace(Tone) ? null : Tone.Trim().ToLowerInvariant(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? null : AriaLabel.Trim()
        };
    }
}
