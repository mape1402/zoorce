namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a compact floating action button.</summary>
public sealed record ZoorceFloatingActionButtonOptions
{
    /// <summary>Gets the unique DOM identifier for the action.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible action label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the icon or glyph shown by the action.</summary>
    public required string Icon { get; init; }

    /// <summary>Gets optional text shown alongside the icon.</summary>
    public string? Text { get; init; }

    /// <summary>Gets the semantic visual tone.</summary>
    public string Tone { get; init; } = "primary";

    /// <summary>Gets whether the action is disabled.</summary>
    public bool Disabled { get; init; }
}
