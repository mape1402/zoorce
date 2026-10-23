namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a collection of contextual actions expanded from one floating trigger.</summary>
public sealed record ZoorceSpeedDialOptions
{
    /// <summary>Gets the unique DOM identifier for the speed dial.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible trigger label.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the trigger icon.</summary>
    public string Icon { get; init; } = "+";

    /// <summary>Gets whether actions expand horizontally instead of vertically.</summary>
    public bool Horizontal { get; init; }

    /// <summary>Gets actions expanded from the trigger.</summary>
    public IReadOnlyList<ZoorceMenuItem> Items { get; init; } = [];
}
