using Zoerce.WebUI.Abstractions.Navigation;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleNavigationProvider : INavigationProvider
{
    public IReadOnlyList<NavigationItem> GetItems() =>
    [
        new("Overview", "/", "01"),
        new("Workspace", Icon: "02", Children:
        [
            new("Dashboard", "/#dashboard", "↗"),
            new("Data grid", "/#data-grid", "▤"),
            new("Forms", "/#forms", "⌘"),
            new("Feedback", "/#feedback", "◉")
        ]),
        new("Documentation", "/#documentation", "03")
    ];
}
