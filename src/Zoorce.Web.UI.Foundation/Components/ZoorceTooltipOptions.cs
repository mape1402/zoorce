namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>
/// Configures passive explanatory content associated with a host-supplied trigger.
/// </summary>
public sealed record ZoorceTooltipOptions
{
    /// <summary>Gets the unique DOM identifier for the tooltip.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the concise explanatory content.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the preferred placement token.</summary>
    public string Placement { get; init; } = "top";

    /// <summary>Normalizes the tooltip contract.</summary>
    public ZoorceTooltipOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A tooltip requires an id.", nameof(Id));
        if (string.IsNullOrWhiteSpace(Text)) throw new ArgumentException("A tooltip requires text.", nameof(Text));
        var placement = Placement?.Trim().ToLowerInvariant();
        placement = placement is "top" or "right" or "bottom" or "left" ? placement : "top";
        return this with { Id = Id.Trim(), Text = Text.Trim(), Placement = placement };
    }
}
