namespace Zoorce.Web.UI.Timelines;

/// <summary>Defines the visual composition used by a timeline instance.</summary>
public enum ZoorceTimelineVariant
{
    /// <summary>Represents the <c>Activity</c> option.</summary>
    Activity,
    /// <summary>Represents the <c>Rail</c> option.</summary>
    Rail,
    /// <summary>Represents the <c>Agenda</c> option.</summary>
    Agenda,
    /// <summary>Represents the <c>Journal</c> option.</summary>
    Journal,
    /// <summary>Represents the <c>Milestones</c> option.</summary>
    Milestones,
    /// <summary>Represents the <c>Presentation</c> option.</summary>
    Presentation
}

/// <summary>Defines the preferred direction of a timeline.</summary>
public enum ZoorceTimelineOrientation
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Vertical</c> option.</summary>
    Vertical,
    /// <summary>Represents the <c>Horizontal</c> option.</summary>
    Horizontal
}

/// <summary>Defines how a horizontal timeline reacts when its content no longer fits its viewport.</summary>
public enum ZoorceTimelineOverflowMode
{
    /// <summary>Keeps the intended composition and exposes an internal horizontal scroller when needed.</summary>
    Auto,

    /// <summary>Always preserves the horizontal composition and exposes an internal horizontal scroller when needed.</summary>
    Scroll,

    /// <summary>Changes an overflowing horizontal composition into its vertical form.</summary>
    Stack
}

/// <summary>Defines the amount of space reserved for timeline content.</summary>
public enum ZoorceTimelineDensity
{
    /// <summary>Represents the <c>Compact</c> option.</summary>
    Compact,
    /// <summary>Represents the <c>Default</c> option.</summary>
    Default,
    /// <summary>Represents the <c>Comfortable</c> option.</summary>
    Comfortable
}

/// <summary>Defines the behavior exposed by an event item.</summary>
public enum ZoorceTimelineInteraction
{
    /// <summary>Represents the <c>ReadOnly</c> option.</summary>
    ReadOnly,
    /// <summary>Represents the <c>Select</c> option.</summary>
    Select,
    /// <summary>Represents the <c>Navigate</c> option.</summary>
    Navigate,
    /// <summary>Represents the <c>Command</c> option.</summary>
    Command
}

/// <summary>Defines the behavior invoked when a timeline item itself is activated.</summary>
public enum ZoorceTimelineItemClickBehavior
{
    /// <summary>Uses the interaction configured for the containing timeline.</summary>
    Inherit,

    /// <summary>Does not make the item activatable.</summary>
    None,

    /// <summary>Selects the item.</summary>
    Select,

    /// <summary>Navigates to the configured target URL.</summary>
    Navigate,

    /// <summary>Raises a command event for the host to handle.</summary>
    Command,

    /// <summary>Raises an item-click event without a default action.</summary>
    Custom
}

/// <summary>Defines how items are grouped in a vertical timeline.</summary>
public enum ZoorceTimelineGroupBy
{
    /// <summary>Represents the <c>None</c> option.</summary>
    None,
    /// <summary>Represents the <c>Day</c> option.</summary>
    Day,
    /// <summary>Represents the <c>Week</c> option.</summary>
    Week,
    /// <summary>Represents the <c>Month</c> option.</summary>
    Month,
    /// <summary>Represents the <c>Year</c> option.</summary>
    Year
}

/// <summary>Defines the semantic emphasis applied to an event.</summary>
public enum ZoorceTimelineTone
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Neutral</c> option.</summary>
    Neutral,
    /// <summary>Represents the <c>Primary</c> option.</summary>
    Primary,
    /// <summary>Represents the <c>Info</c> option.</summary>
    Info,
    /// <summary>Represents the <c>Success</c> option.</summary>
    Success,
    /// <summary>Represents the <c>Warning</c> option.</summary>
    Warning,
    /// <summary>Represents the <c>Danger</c> option.</summary>
    Danger
}

/// <summary>Defines the lifecycle state represented by an event.</summary>
public enum ZoorceTimelineState
{
    /// <summary>Represents the <c>Default</c> option.</summary>
    Default,
    /// <summary>Represents the <c>Highlighted</c> option.</summary>
    Highlighted,
    /// <summary>Represents the <c>Pending</c> option.</summary>
    Pending,
    /// <summary>Represents the <c>Completed</c> option.</summary>
    Completed,
    /// <summary>Represents the <c>Error</c> option.</summary>
    Error
}

/// <summary>Defines the placement an event prefers in a composed timeline.</summary>
public enum ZoorceTimelinePlacement
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Left</c> option.</summary>
    Left,
    /// <summary>Represents the <c>Right</c> option.</summary>
    Right,
    /// <summary>Represents the <c>Above</c> option.</summary>
    Above,
    /// <summary>Represents the <c>Below</c> option.</summary>
    Below
}

/// <summary>Defines the marker rendered on a timeline rail.</summary>
public enum ZoorceTimelineMarkerStyle
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Dot</c> option.</summary>
    Dot,
    /// <summary>Represents the <c>Icon</c> option.</summary>
    Icon,
    /// <summary>Represents the <c>Number</c> option.</summary>
    Number,
    /// <summary>Represents the <c>Ring</c> option.</summary>
    Ring
}

/// <summary>Defines the connector used by a timeline rail.</summary>
public enum ZoorceTimelineConnectorStyle
{
    /// <summary>Represents the <c>Solid</c> option.</summary>
    Solid,
    /// <summary>Represents the <c>Dashed</c> option.</summary>
    Dashed,
    /// <summary>Represents the <c>Segmented</c> option.</summary>
    Segmented,
    /// <summary>Represents the <c>Curved</c> option.</summary>
    Curved,
    /// <summary>Represents the <c>None</c> option.</summary>
    None
}

/// <summary>Defines optional animation used to emphasize a current event.</summary>
public enum ZoorceTimelineActiveMotion
{
    /// <summary>Represents the <c>None</c> option.</summary>
    None,
    /// <summary>Represents the <c>HaloPulse</c> option.</summary>
    HaloPulse,
    /// <summary>Represents the <c>Reveal</c> option.</summary>
    Reveal,
    /// <summary>Represents the <c>MarkerPulse</c> option.</summary>
    MarkerPulse
}

/// <summary>Defines the rendering strategy used for a remote source refresh.</summary>
public enum ZoorceTimelineRefreshMode
{
    /// <summary>Represents the <c>Replace</c> option.</summary>
    Replace,
    /// <summary>Represents the <c>Merge</c> option.</summary>
    Merge
}

/// <summary>Describes the person or system associated with a timeline item.</summary>
public sealed record ZoorceTimelineActor(string Name, string? Initials = null, string? AvatarUrl = null, string? Role = null);

/// <summary>Describes an optional command offered by a timeline event.</summary>
public sealed record ZoorceTimelineAction(string Id, string Label, string? Icon = null, string? TargetUrl = null);

/// <summary>Describes one independently addressable timeline event.</summary>
public sealed record ZoorceTimelineItem(
    string Id,
    DateTimeOffset Timestamp,
    string Title,
    string? Description = null,
    string? Icon = null,
    ZoorceTimelineTone Tone = ZoorceTimelineTone.Auto,
    ZoorceTimelineState State = ZoorceTimelineState.Default,
    ZoorceTimelinePlacement Placement = ZoorceTimelinePlacement.Auto,
    string? Flag = null,
    ZoorceTimelineActor? Actor = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? Duration = null,
    string? Location = null,
    IReadOnlyList<ZoorceTimelineActor>? Participants = null,
    ZoorceTimelineAction? Action = null,
    bool IsRead = true,
    bool IsPinned = false,
    bool IsSelectable = true,
    string? TemplateKey = null,
    ZoorceTimelineItemClickBehavior ClickBehavior = ZoorceTimelineItemClickBehavior.Inherit,
    string? CommandId = null,
    string? TargetUrl = null);

/// <summary>Maps an item template key to its server-rendered and client-side representations.</summary>
public sealed record ZoorceTimelineItemTemplate(string Key, string PartialViewName, string? ClientTemplateId = null);

/// <summary>Supplies normalized timeline data to a custom item partial.</summary>
public sealed record ZoorceTimelineItemTemplateContext(
    ZoorceTimelineOptions Timeline,
    ZoorceTimelineItem Item,
    int Index,
    bool IsSelected,
    string TimestampText,
    string? RelativeTime,
    ZoorceTimelineItemClickBehavior ClickBehavior)
{
    /// <summary>Returns a normalized metadata value when the template needs a domain-specific field.</summary>
    public string? Metadata(string key) => Item.Metadata is not null && Item.Metadata.TryGetValue(key, out var value) ? value : null;
}

/// <summary>Groups the visual decisions shared by one timeline instance.</summary>
public sealed record ZoorceTimelinePresentation
{
    /// <summary>Gets the marker appearance used by the instance.</summary>
    public ZoorceTimelineMarkerStyle MarkerStyle { get; init; } = ZoorceTimelineMarkerStyle.Auto;

    /// <summary>Gets the connector appearance used by the instance.</summary>
    public ZoorceTimelineConnectorStyle ConnectorStyle { get; init; } = ZoorceTimelineConnectorStyle.Solid;

    /// <summary>Gets the active event animation.</summary>
    public ZoorceTimelineActiveMotion ActiveMotion { get; init; } = ZoorceTimelineActiveMotion.None;

    /// <summary>Gets whether event content is rendered in a surfaced card.</summary>
    public bool UseCards { get; init; } = true;

}
