using Microsoft.AspNetCore.Html;

namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a persistent, filterable notification center.</summary>
public sealed record ZoorceNotificationCenterOptions
{
    /// <summary>Gets the unique DOM identifier for the center.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible label for the trigger and panel.</summary>
    public string Label { get; init; } = "Notifications";

    /// <summary>Gets the text rendered when no notification matches the active filter.</summary>
    public string EmptyText { get; init; } = "You are all caught up.";

    /// <summary>Gets optional host markup that replaces the default bell icon in the trigger.</summary>
    public IHtmlContent? TriggerIcon { get; init; }

    /// <summary>Gets whether an unread filter is rendered.</summary>
    public bool ShowUnreadFilter { get; init; } = true;

    /// <summary>Gets notification items in display order.</summary>
    public IReadOnlyList<ZoorceNotificationItem> Items { get; init; } = [];

    /// <summary>Gets optional host markup used for the contents of each notification item.</summary>
    /// <remarks>The component retains each item's semantic wrapper, read state and filtering. Templates can opt into the action event with a <c>data-z-notification-action</c> element.</remarks>
    public Func<ZoorceNotificationItem, IHtmlContent>? ItemTemplate { get; init; }

    /// <summary>Validates and normalizes the notification-center configuration.</summary>
    public ZoorceNotificationCenterOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A notification center requires an id.", nameof(Id));
        var normalizedItems = Items.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            if (string.IsNullOrWhiteSpace(item.Id)) throw new ArgumentException("Every notification requires an id.", nameof(Items));
            if (string.IsNullOrWhiteSpace(item.Title)) throw new ArgumentException("Every notification requires a title.", nameof(Items));
            return item with
            {
                Id = item.Id.Trim(),
                Title = item.Title.Trim(),
                Description = TrimOrNull(item.Description),
                Icon = TrimOrNull(item.Icon),
                ActionText = TrimOrNull(item.ActionText),
                ActionHref = TrimOrNull(item.ActionHref),
                Tone = Enum.IsDefined(item.Tone) ? item.Tone : ZoorceNotificationTone.Info
            };
        }).ToArray();
        if (normalizedItems.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedItems.Length) throw new ArgumentException("Notification ids must be unique.", nameof(Items));
        return this with
        {
            Id = Id.Trim(),
            Label = string.IsNullOrWhiteSpace(Label) ? "Notifications" : Label.Trim(),
            EmptyText = string.IsNullOrWhiteSpace(EmptyText) ? "You are all caught up." : EmptyText.Trim(),
            Items = normalizedItems
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
