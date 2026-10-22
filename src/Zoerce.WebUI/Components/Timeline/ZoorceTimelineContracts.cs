namespace Zoerce.WebUI.Components.Timeline;

/// <summary>Defines the visual composition used by a timeline instance.</summary>
public enum ZoorceTimelineVariant
{
    Activity,
    Rail,
    Agenda,
    Journal,
    Milestones,
    Presentation
}

/// <summary>Defines the preferred direction of a timeline.</summary>
public enum ZoorceTimelineOrientation
{
    Auto,
    Vertical,
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
    Compact,
    Default,
    Comfortable
}

/// <summary>Defines the behavior exposed by an event item.</summary>
public enum ZoorceTimelineInteraction
{
    ReadOnly,
    Select,
    Navigate,
    Command
}

/// <summary>Defines how items are grouped in a vertical timeline.</summary>
public enum ZoorceTimelineGroupBy
{
    None,
    Day,
    Week,
    Month,
    Year
}

/// <summary>Defines the semantic emphasis applied to an event.</summary>
public enum ZoorceTimelineTone
{
    Auto,
    Neutral,
    Primary,
    Info,
    Success,
    Warning,
    Danger
}

/// <summary>Defines the lifecycle state represented by an event.</summary>
public enum ZoorceTimelineState
{
    Default,
    Highlighted,
    Pending,
    Completed,
    Error
}

/// <summary>Defines the placement an event prefers in a composed timeline.</summary>
public enum ZoorceTimelinePlacement
{
    Auto,
    Left,
    Right,
    Above,
    Below
}

/// <summary>Defines the marker rendered on a timeline rail.</summary>
public enum ZoorceTimelineMarkerStyle
{
    Auto,
    Dot,
    Icon,
    Number,
    Ring
}

/// <summary>Defines the connector used by a timeline rail.</summary>
public enum ZoorceTimelineConnectorStyle
{
    Solid,
    Dashed,
    Segmented,
    Curved,
    None
}

/// <summary>Defines optional animation used to emphasize a current event.</summary>
public enum ZoorceTimelineActiveMotion
{
    None,
    HaloPulse,
    Reveal,
    MarkerPulse
}

/// <summary>Defines the rendering strategy used for a remote source refresh.</summary>
public enum ZoorceTimelineRefreshMode
{
    Replace,
    Merge
}

/// <summary>Defines the SignalR transport preferred by a timeline subscription.</summary>
public enum ZoorceTimelineRealtimeTransport
{
    /// <summary>Lets the client negotiate the best available transport.</summary>
    Auto,

    /// <summary>Uses WebSockets where the host and browser support them.</summary>
    WebSockets,

    /// <summary>Uses server-sent events where WebSockets are unavailable.</summary>
    ServerSentEvents,

    /// <summary>Uses HTTP long polling where persistent browser connections are unavailable.</summary>
    LongPolling
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
    bool IsSelectable = true);

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
