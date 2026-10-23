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
            Icon = string.IsNullOrWhiteSpace(Icon) ? null : Icon.Trim()
        };
    }
}
