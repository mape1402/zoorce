namespace Zoerce.WebUI.Components.Timeline;

/// <summary>Configures an isolated, progressively enhanced timeline.</summary>
public sealed record ZoorceTimelineOptions
{
    /// <summary>Gets the unique DOM identifier for the component.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the accessible name announced for the timeline.</summary>
    public string AriaLabel { get; init; } = "Timeline";

    /// <summary>Gets the events rendered by the server before client-side enhancement.</summary>
    public IReadOnlyList<ZoorceTimelineItem> Items { get; init; } = [];

    /// <summary>Gets the identifier of the selected event, when selection is enabled.</summary>
    public string? SelectedItemId { get; init; }

    /// <summary>Gets the composition used by this timeline.</summary>
    public ZoorceTimelineVariant Variant { get; init; } = ZoorceTimelineVariant.Activity;

    /// <summary>Gets the preferred direction of the timeline.</summary>
    public ZoorceTimelineOrientation Orientation { get; init; } = ZoorceTimelineOrientation.Auto;

    /// <summary>Gets how a horizontal timeline behaves when its content exceeds the available inline space.</summary>
    public ZoorceTimelineOverflowMode OverflowMode { get; init; } = ZoorceTimelineOverflowMode.Auto;

    /// <summary>Gets whether horizontal overflow controls are rendered when the timeline needs them.</summary>
    public bool ShowOverflowControls { get; init; } = true;

    /// <summary>Gets the interaction offered by this instance.</summary>
    public ZoorceTimelineInteraction Interaction { get; init; } = ZoorceTimelineInteraction.ReadOnly;

    /// <summary>Gets the density used for event content and spacing.</summary>
    public ZoorceTimelineDensity Density { get; init; } = ZoorceTimelineDensity.Default;

    /// <summary>Gets the grouping applied to event timestamps.</summary>
    public ZoorceTimelineGroupBy GroupBy { get; init; } = ZoorceTimelineGroupBy.None;

    /// <summary>Gets whether the oldest event is rendered first.</summary>
    public bool SortAscending { get; init; } = true;

    /// <summary>Gets whether an absolute timestamp is displayed for each item.</summary>
    public bool ShowTimestamp { get; init; } = true;

    /// <summary>Gets whether relative time is displayed for each item.</summary>
    public bool ShowRelativeTime { get; init; }

    /// <summary>Gets whether actor information is visible.</summary>
    public bool ShowActors { get; init; } = true;

    /// <summary>Gets whether metadata and tags are visible.</summary>
    public bool ShowMetadata { get; init; } = true;

    /// <summary>Gets whether the component follows newly received events.</summary>
    public bool FollowLatest { get; init; }

    /// <summary>Gets the optional maximum number of events retained by live updates.</summary>
    public int? MaxItems { get; init; }

    /// <summary>Gets the visual options shared by every event in this instance.</summary>
    public ZoorceTimelinePresentation Presentation { get; init; } = new();

    /// <summary>Gets an optional same-origin endpoint that returns a timeline snapshot.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Gets whether a refresh replaces or merges the supplied snapshot.</summary>
    public ZoorceTimelineRefreshMode RefreshMode { get; init; } = ZoorceTimelineRefreshMode.Merge;

    /// <summary>Gets an optional SignalR hub endpoint used for live events.</summary>
    public string? RealtimeHubUrl { get; init; }

    /// <summary>Gets the SignalR event name listened to by this timeline.</summary>
    public string? RealtimeEventName { get; init; }

    /// <summary>Gets the preferred transport used for the optional SignalR subscription.</summary>
    public ZoorceTimelineRealtimeTransport RealtimeTransport { get; init; } = ZoorceTimelineRealtimeTransport.Auto;

    /// <summary>Gets the optional text shown when no events are available.</summary>
    public string EmptyMessage { get; init; } = "No events to display.";

    /// <summary>Validates and normalizes host-supplied configuration.</summary>
    public ZoorceTimelineOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A timeline requires a unique id.", nameof(Id));
        }

        var items = Items ?? throw new ArgumentNullException(nameof(Items));
        var normalizedItems = items.Select(NormalizeItem).OrderBy(item => item.Timestamp).ToArray();
        if (!SortAscending)
        {
            Array.Reverse(normalizedItems);
        }

        if (normalizedItems.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedItems.Length)
        {
            throw new ArgumentException("Timeline item ids must be unique.", nameof(Items));
        }

        var selected = TrimOrNull(SelectedItemId);
        if (selected is not null && !normalizedItems.Any(item => string.Equals(item.Id, selected, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The selected event must identify one configured item.", nameof(SelectedItemId));
        }

        var variant = Enum.IsDefined(Variant) ? Variant : ZoorceTimelineVariant.Activity;
        var orientation = Enum.IsDefined(Orientation) ? Orientation : ZoorceTimelineOrientation.Auto;

        return this with
        {
            Id = Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Timeline" : AriaLabel.Trim(),
            Items = normalizedItems,
            SelectedItemId = selected,
            Variant = variant,
            Orientation = orientation,
            OverflowMode = Enum.IsDefined(OverflowMode) ? OverflowMode : ZoorceTimelineOverflowMode.Auto,
            Interaction = Enum.IsDefined(Interaction) ? Interaction : ZoorceTimelineInteraction.ReadOnly,
            Density = Enum.IsDefined(Density) ? Density : ZoorceTimelineDensity.Default,
            GroupBy = Enum.IsDefined(GroupBy) ? GroupBy : ZoorceTimelineGroupBy.None,
            MaxItems = MaxItems is > 0 ? MaxItems : null,
            Presentation = NormalizePresentation(Presentation),
            SourceUrl = TrimOrNull(SourceUrl),
            RefreshMode = Enum.IsDefined(RefreshMode) ? RefreshMode : ZoorceTimelineRefreshMode.Merge,
            RealtimeHubUrl = TrimOrNull(RealtimeHubUrl),
            RealtimeEventName = TrimOrNull(RealtimeEventName),
            RealtimeTransport = Enum.IsDefined(RealtimeTransport) ? RealtimeTransport : ZoorceTimelineRealtimeTransport.Auto,
            EmptyMessage = string.IsNullOrWhiteSpace(EmptyMessage) ? "No events to display." : EmptyMessage.Trim()
        };
    }

    private static ZoorceTimelineItem NormalizeItem(ZoorceTimelineItem? item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            throw new ArgumentException("Every timeline item requires an id.", nameof(Items));
        }

        var tags = (item.Tags ?? []).Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var metadata = (item.Metadata ?? new Dictionary<string, string>())
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
        return item with
        {
            Id = item.Id.Trim(),
            Title = string.IsNullOrWhiteSpace(item.Title) ? "Event" : item.Title.Trim(),
            Description = TrimOrNull(item.Description),
            Icon = TrimOrNull(item.Icon),
            Flag = TrimOrNull(item.Flag),
            Duration = TrimOrNull(item.Duration),
            Location = TrimOrNull(item.Location),
            Tone = Enum.IsDefined(item.Tone) ? item.Tone : ZoorceTimelineTone.Auto,
            State = Enum.IsDefined(item.State) ? item.State : ZoorceTimelineState.Default,
            Placement = Enum.IsDefined(item.Placement) ? item.Placement : ZoorceTimelinePlacement.Auto,
            Actor = NormalizeActor(item.Actor),
            Participants = (item.Participants ?? []).Where(actor => actor is not null).Select(NormalizeActor).Cast<ZoorceTimelineActor>().ToArray(),
            Tags = tags,
            Metadata = metadata,
            Action = NormalizeAction(item.Action)
        };
    }

    private static ZoorceTimelineActor? NormalizeActor(ZoorceTimelineActor? actor) => actor is null || string.IsNullOrWhiteSpace(actor.Name)
        ? null
        : actor with
        {
            Name = actor.Name.Trim(),
            Initials = TrimOrNull(actor.Initials),
            AvatarUrl = TrimOrNull(actor.AvatarUrl),
            Role = TrimOrNull(actor.Role)
        };

    private static ZoorceTimelineAction? NormalizeAction(ZoorceTimelineAction? action) => action is null || string.IsNullOrWhiteSpace(action.Id) || string.IsNullOrWhiteSpace(action.Label)
        ? null
        : action with { Id = action.Id.Trim(), Label = action.Label.Trim(), Icon = TrimOrNull(action.Icon), TargetUrl = TrimOrNull(action.TargetUrl) };

    private static ZoorceTimelinePresentation NormalizePresentation(ZoorceTimelinePresentation? presentation)
    {
        var value = presentation ?? new ZoorceTimelinePresentation();
        return value with
        {
            MarkerStyle = Enum.IsDefined(value.MarkerStyle) ? value.MarkerStyle : ZoorceTimelineMarkerStyle.Auto,
            ConnectorStyle = Enum.IsDefined(value.ConnectorStyle) ? value.ConnectorStyle : ZoorceTimelineConnectorStyle.Solid,
            ActiveMotion = Enum.IsDefined(value.ActiveMotion) ? value.ActiveMotion : ZoorceTimelineActiveMotion.None
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
