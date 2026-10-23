namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a compact label, count, or status dot.</summary>
public sealed record ZoorceBadgeOptions
{
    /// <summary>Gets optional visible badge text.</summary>
    public string? Text { get; init; }

    /// <summary>Gets an optional numeric value.</summary>
    public int? Count { get; init; }

    /// <summary>Gets the maximum displayed numeric value before a plus suffix is used.</summary>
    public int MaximumCount { get; init; } = 99;

    /// <summary>Gets whether the component renders as a compact status dot.</summary>
    public bool Dot { get; init; }

    /// <summary>Gets the semantic visual tone.</summary>
    public string Tone { get; init; } = "primary";

    /// <summary>Gets the accessible label, required for a dot-only badge.</summary>
    public string? AriaLabel { get; init; }

    /// <summary>Gets the rendered visible value.</summary>
    public string GetDisplayValue()
        => Count is null ? Text ?? string.Empty : Count.Value > Math.Max(1, MaximumCount) ? $"{Math.Max(1, MaximumCount)}+" : Count.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
