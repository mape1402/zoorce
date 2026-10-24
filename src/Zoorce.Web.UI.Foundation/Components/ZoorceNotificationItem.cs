namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Describes one notification displayed by a notification center.</summary>
public sealed record ZoorceNotificationItem
{
    /// <summary>Gets the stable item identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the notification title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets optional contextual copy.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the time associated with the notification.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets whether the visitor has read the notification.</summary>
    public bool IsRead { get; init; }

    /// <summary>Gets the semantic emphasis applied to the notification.</summary>
    public ZoorceNotificationTone Tone { get; init; } = ZoorceNotificationTone.Info;

    /// <summary>Gets an optional decorative icon.</summary>
    public string? Icon { get; init; }

    /// <summary>Gets optional action text.</summary>
    public string? ActionText { get; init; }

    /// <summary>Gets an optional action URL.</summary>
    public string? ActionHref { get; init; }
}
