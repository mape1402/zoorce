namespace Zoerce.WebUI.Components.Forms;

/// <summary>Configures a presentational or removable chip.</summary>
public sealed record ZoorceChipOptions
{
    public string Text { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public string? Tone { get; init; }
    public bool Removable { get; init; }
    public bool Selected { get; init; }
    public bool Disabled { get; init; }
    public string? AriaLabel { get; init; }

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
