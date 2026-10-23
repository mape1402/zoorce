namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures an accessible menu anchored to a button-like trigger.</summary>
public sealed record ZoorceDropdownMenuOptions
{
    /// <summary>Gets the unique DOM identifier for the menu.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the trigger label.</summary>
    public required string Text { get; init; }

    /// <summary>Gets an optional trigger icon.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets the semantic trigger tone.</summary>
    public string Tone { get; init; } = "ghost";

    /// <summary>Gets whether the menu aligns its trailing edge to the trigger.</summary>
    public bool AlignEnd { get; init; }

    /// <summary>Gets entries rendered by the menu.</summary>
    public IReadOnlyList<ZoorceMenuItem> Items { get; init; } = [];

    /// <summary>Validates and normalizes the menu configuration.</summary>
    public ZoorceDropdownMenuOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A dropdown menu requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Text)) throw new ArgumentException("A dropdown menu requires trigger text.", nameof(Text));
        return this with { Id = Id.Trim(), Text = Text.Trim(), Tone = string.IsNullOrWhiteSpace(Tone) ? "ghost" : Tone.Trim().ToLowerInvariant() };
    }
}
