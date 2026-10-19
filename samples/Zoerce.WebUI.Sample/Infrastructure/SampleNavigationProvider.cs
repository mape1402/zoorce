using Zoerce.WebUI.Abstractions.Navigation;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleNavigationProvider : INavigationProvider
{
    public IReadOnlyList<NavigationItem> GetItems() =>
    [
        new("Overview", "/", "01", MatchMode: NavigationMatchMode.Exact),
        new("Workspace", Icon: "02", Children:
        [
            new("Dashboard", "/dashboard", "↗", MatchMode: NavigationMatchMode.Exact),
            new("Components", "/components", "▤", MatchMode: NavigationMatchMode.Exact),
            new("Forms", "/forms", "⌘", MatchMode: NavigationMatchMode.Exact),
            new("Feedback", "/feedback", "◉", MatchMode: NavigationMatchMode.Exact)
        ]),
        new("Documentation", "/documentation", "03", MatchMode: NavigationMatchMode.Exact)
    ];
}
