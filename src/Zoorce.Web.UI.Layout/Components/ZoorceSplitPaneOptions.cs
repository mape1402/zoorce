using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Layout;

/// <summary>Configures a user-resizable pair of application panes.</summary>
public sealed record ZoorceSplitPaneOptions
{
    /// <summary>Gets the unique DOM identifier for the split pane.</summary>
    public required string Id { get; init; }

    /// <summary>Gets content for the first pane.</summary>
    public required IHtmlContent Primary { get; init; }

    /// <summary>Gets content for the second pane.</summary>
    public required IHtmlContent Secondary { get; init; }

    /// <summary>Gets the axis on which the divider is resized.</summary>
    public ZoorceSplitOrientation Orientation { get; init; } = ZoorceSplitOrientation.Horizontal;

    /// <summary>Gets the initial percentage occupied by the first pane.</summary>
    public double InitialPrimaryPercent { get; init; } = 50;

    /// <summary>Gets the smallest allowed percentage for the first pane.</summary>
    public double MinimumPrimaryPercent { get; init; } = 20;

    /// <summary>Gets the smallest allowed percentage for the second pane.</summary>
    public double MinimumSecondaryPercent { get; init; } = 20;

    /// <summary>Gets whether the first pane can be collapsed from the divider.</summary>
    public bool Collapsible { get; init; }

    /// <summary>Gets whether the first pane starts collapsed.</summary>
    public bool InitiallyCollapsed { get; init; }

    /// <summary>Gets an optional browser storage key used to retain the user's size choice.</summary>
    public string? PersistenceKey { get; init; }

    /// <summary>Gets optional CSS classes applied to the outer split pane.</summary>
    public string? CssClass { get; init; }

    /// <summary>Validates and normalizes the split-pane configuration.</summary>
    public ZoorceSplitPaneOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A split pane requires an id.", nameof(Id));
        ArgumentNullException.ThrowIfNull(Primary);
        ArgumentNullException.ThrowIfNull(Secondary);
        if (!Enum.IsDefined(Orientation)) throw new ArgumentOutOfRangeException(nameof(Orientation));
        var minimumPrimary = Math.Clamp(MinimumPrimaryPercent, 5, 90);
        var minimumSecondary = Math.Clamp(MinimumSecondaryPercent, 5, 90);
        if (minimumPrimary + minimumSecondary > 95) throw new ArgumentException("Pane minimums leave no usable resize range.", nameof(MinimumPrimaryPercent));
        return this with
        {
            Id = Id.Trim(),
            InitialPrimaryPercent = Math.Clamp(InitialPrimaryPercent, minimumPrimary, 100 - minimumSecondary),
            MinimumPrimaryPercent = minimumPrimary,
            MinimumSecondaryPercent = minimumSecondary,
            PersistenceKey = TrimOrNull(PersistenceKey),
            CssClass = TrimOrNull(CssClass)
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value)
        ? null
        : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
