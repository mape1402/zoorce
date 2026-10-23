using Zoorce.Web.UI.Foundation.Components;

namespace Zoorce.Web.UI.Navigation.Components;

internal static class ZoorceNavigationNormalizers
{
    internal static IReadOnlyList<ZoorceAccordionItem> AccordionItems(IReadOnlyList<ZoorceAccordionItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items.Select(item =>
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Title)) throw new ArgumentException("Every accordion item requires an id and title.", nameof(items));
            return item with { Id = item.Id.Trim(), Title = item.Title.Trim(), Description = Text(item.Description) };
        }).ToArray();
        EnsureDistinct(normalized.Select(item => item.Id), "accordion item", nameof(items));
        return normalized;
    }

    internal static IReadOnlyList<ZoorceTabItem> Tabs(IReadOnlyList<ZoorceTabItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items.Select(item =>
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Text)) throw new ArgumentException("Every tab requires an id and text.", nameof(items));
            return item with { Id = item.Id.Trim(), Text = item.Text.Trim(), Icon = Text(item.Icon), Badge = Text(item.Badge) };
        }).ToArray();
        EnsureDistinct(normalized.Select(item => item.Id), "tab", nameof(items));
        return normalized;
    }

    internal static IReadOnlyList<ZoorceMenuItem> MenuItems(IReadOnlyList<ZoorceMenuItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items.Select(item =>
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Value) || string.IsNullOrWhiteSpace(item.Text)) throw new ArgumentException("Every menu item requires a value and text.", nameof(items));
            return item with { Value = item.Value.Trim(), Text = item.Text.Trim(), Description = Text(item.Description), Icon = Text(item.Icon), Href = Text(item.Href), Shortcut = Text(item.Shortcut), Children = MenuItems(item.Children) };
        }).ToArray();
        EnsureDistinct(normalized.Select(item => item.Value), "menu item", nameof(items));
        return normalized;
    }

    internal static IReadOnlyList<ZoorceTreeNode> TreeNodes(IReadOnlyList<ZoorceTreeNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var normalized = items.Select(item =>
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Value) || string.IsNullOrWhiteSpace(item.Text)) throw new ArgumentException("Every tree node requires a value and text.", nameof(items));
            return item with { Value = item.Value.Trim(), Text = item.Text.Trim(), Icon = Text(item.Icon), Children = TreeNodes(item.Children) };
        }).ToArray();
        EnsureDistinct(normalized.Select(item => item.Value), "tree node", nameof(items));
        return normalized;
    }

    private static void EnsureDistinct(IEnumerable<string> values, string itemType, string parameterName)
    {
        var duplicate = values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null) throw new ArgumentException($"The {itemType} '{duplicate.Key}' is duplicated.", parameterName);
    }

    private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
