namespace Zoorce.Web.UI.Navigation.Components;

/// <summary>Configures a responsive breadcrumb trail.</summary>
public sealed record ZoorceBreadcrumbOptions
{
    /// <summary>Gets the unique DOM identifier for the breadcrumb trail.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the accessible label announced for the trail.</summary>
    public string AriaLabel { get; init; } = "Breadcrumb";

    /// <summary>Gets locations in hierarchy order.</summary>
    public IReadOnlyList<ZoorceBreadcrumbItem> Items { get; init; } = [];

    /// <summary>Gets the maximum number of visible locations before middle locations collapse.</summary>
    public int MaxVisible { get; init; } = 4;

    /// <summary>Validates and normalizes the breadcrumb configuration.</summary>
    public ZoorceBreadcrumbOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("A breadcrumb requires an id.", nameof(Id));
        if (Items.Count == 0) throw new ArgumentException("A breadcrumb requires at least one item.", nameof(Items));

        var currentIndex = -1;
        for (var index = 0; index < Items.Count; index++)
        {
            if (Items[index]?.Current != true) continue;
            if (currentIndex >= 0) throw new ArgumentException("A breadcrumb can have only one current item.", nameof(Items));
            currentIndex = index;
        }
        if (currentIndex < 0) currentIndex = Items.Count - 1;
        var normalizedItems = Items.Select((item, index) =>
        {
            ArgumentNullException.ThrowIfNull(item);
            if (string.IsNullOrWhiteSpace(item.Text)) throw new ArgumentException("Breadcrumb item text is required.", nameof(Items));
            return item with
            {
                Text = item.Text.Trim(),
                Href = string.IsNullOrWhiteSpace(item.Href) ? null : item.Href.Trim(),
                Icon = string.IsNullOrWhiteSpace(item.Icon) ? null : item.Icon.Trim(),
                Current = index == currentIndex
            };
        }).ToArray();

        return this with
        {
            Id = Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Breadcrumb" : AriaLabel.Trim(),
            Items = normalizedItems,
            MaxVisible = Math.Clamp(MaxVisible, 2, 10)
        };
    }
}
