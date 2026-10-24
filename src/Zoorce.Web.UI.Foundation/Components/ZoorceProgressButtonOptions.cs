namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a button whose surface itself visualizes progress.</summary>
public sealed record ZoorceProgressButtonOptions
{
    /// <summary>Gets the unique DOM identifier for the action.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the fixed, visible action label.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the progress percentage from zero through one hundred.</summary>
    public double? Progress { get; init; }

    /// <summary>Gets whether an unknown-duration operation renders an animated fill.</summary>
    public bool IsIndeterminate { get; init; }

    /// <summary>Gets whether user interaction is disabled while progress is active.</summary>
    public bool DisableWhileActive { get; init; } = true;

    /// <summary>Gets the semantic button tone.</summary>
    public string Tone { get; init; } = "primary";

    /// <summary>Gets the semantic tone rendered after progress reaches one hundred percent.</summary>
    public string CompletionTone { get; init; } = "success";

    /// <summary>Gets an optional CSS color that overrides the active progress tone for this instance.</summary>
    public string? ActiveColor { get; init; }

    /// <summary>Gets an optional CSS color placed on the active progress tone for this instance.</summary>
    public string? ActiveForegroundColor { get; init; }

    /// <summary>Gets an optional CSS color that overrides the completion tone for this instance.</summary>
    public string? CompletionColor { get; init; }

    /// <summary>Gets an optional CSS color placed on the completion tone for this instance.</summary>
    public string? CompletionForegroundColor { get; init; }

    /// <summary>Gets an optional leading icon that remains visible above the fill.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets whether the action begins disabled.</summary>
    public bool Disabled { get; init; }

    /// <summary>Validates and normalizes the progress button configuration.</summary>
    public ZoorceProgressButtonOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A progress button requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Text)) throw new ArgumentException("A progress button requires text.", nameof(Text));
        return this with
        {
            Id = Id.Trim(),
            Text = Text.Trim(),
            Progress = Progress is null ? null : Math.Clamp(Progress.Value, 0, 100),
            Tone = string.IsNullOrWhiteSpace(Tone) ? "primary" : Tone.Trim().ToLowerInvariant(),
            CompletionTone = string.IsNullOrWhiteSpace(CompletionTone) ? "success" : CompletionTone.Trim().ToLowerInvariant(),
            ActiveColor = NormalizeCssColor(ActiveColor, nameof(ActiveColor)),
            ActiveForegroundColor = NormalizeCssColor(ActiveForegroundColor, nameof(ActiveForegroundColor)),
            CompletionColor = NormalizeCssColor(CompletionColor, nameof(CompletionColor)),
            CompletionForegroundColor = NormalizeCssColor(CompletionForegroundColor, nameof(CompletionForegroundColor)),
            Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim()
        };
    }

    private static string? NormalizeCssColor(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var color = value.Trim();
        if (color.IndexOfAny([';', '{', '}', '<', '>', '"', '\'']) >= 0)
        {
            throw new ArgumentException("A custom progress color must be a single CSS color value.", parameterName);
        }

        return color;
    }
}
