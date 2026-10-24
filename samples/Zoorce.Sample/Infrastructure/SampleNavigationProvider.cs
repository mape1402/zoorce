using Zoorce.Web.UI.Abstractions.Navigation;

namespace Zoorce.Sample.Infrastructure;

public sealed class SampleNavigationProvider : INavigationProvider
{
    public IReadOnlyList<NavigationItem> GetItems() =>
    [
        new("Overview", "/", "01", MatchMode: NavigationMatchMode.Exact),
        new("Workspace", Icon: "02", Id: "workspace", GroupExpansion: NavigationGroupExpansion.Expanded, Children:
        [
            new("Dashboard", "/dashboard", "↗", MatchMode: NavigationMatchMode.Exact),
            new("Delivery", Icon: "↗", Children:
            [
                LabItem("Release queue", "release-queue", "RQ"),
                LabItem("Milestones", "milestones", "MS"),
                LabItem("Roadmap", "roadmap", "RM"),
                LabItem("Reviews", "reviews", "RV"),
                LabItem("Risks", "risks", "RK")
            ]),
            new("Build", Icon: "▤", Id: "workspace-build", GroupExpansion: NavigationGroupExpansion.Expanded, Children:
            [
                new("Components", "/components", "▤", MatchMode: NavigationMatchMode.Exact),
                new("Charts", "/charts", "◒", MatchMode: NavigationMatchMode.Exact),
                new("Diagrams", "/diagrams", "◇", MatchMode: NavigationMatchMode.Exact),
                new("Steppers", "/steppers", "⇢", MatchMode: NavigationMatchMode.Exact),
                new("Wizards", "/wizards", "↳", MatchMode: NavigationMatchMode.Exact),
                new("Timelines", "/timeline", "◷", MatchMode: NavigationMatchMode.Exact),
                new("Infinite grid", "/data-grid", "∞", MatchMode: NavigationMatchMode.Exact),
                new("Forms", "/forms", "⌘", MatchMode: NavigationMatchMode.Exact),
                new("Navigation", "/navigation", "↔", MatchMode: NavigationMatchMode.Exact),
                LabItem("Patterns", "patterns", "PT"),
                LabItem("Tokens", "tokens", "TK"),
                LabItem("Accessibility", "accessibility", "AA")
            ]),
            new("Feedback", "/feedback", "◉", MatchMode: NavigationMatchMode.Exact)
        ]),
        new("Operations", Icon: "03", Children:
        [
            new("Observability", Icon: "◉", Children:
            [
                LabItem("Service health", "service-health", "SH"),
                LabItem("Incidents", "incidents", "IN"),
                LabItem("Alerts", "alerts", "AL"),
                LabItem("Usage", "usage", "US"),
                LabItem("Capacity", "capacity", "CP")
            ]),
            new("Governance", Icon: "◇", Children:
            [
                LabItem("Policies", "policies", "PL"),
                LabItem("Audit log", "audit-log", "AU"),
                LabItem("Retention", "retention", "RT"),
                LabItem("Approvals", "approvals", "AP"),
                LabItem("Compliance", "compliance", "CO")
            ]),
            new("Access", Icon: "⌁", Children:
            [
                LabItem("Members", "members", "MB"),
                LabItem("Roles", "roles", "RL"),
                LabItem("Sessions", "sessions", "SS"),
                LabItem("API access", "api-access", "API"),
                LabItem("Service accounts", "service-accounts", "SA")
            ])
        ]),
        new("Library", Icon: "04", Children:
        [
            new("Foundations", Icon: "◇", Children:
            [
                LabItem("Colors", "colors", "CL"),
                LabItem("Typography", "typography", "TY"),
                LabItem("Spacing", "spacing", "SP"),
                LabItem("Icons", "icons", "IC"),
                LabItem("Motion", "motion", "MO")
            ])
        ]),
        new("Documentation", "/documentation", "05", MatchMode: NavigationMatchMode.Exact)
    ];

    private static NavigationItem LabItem(string text, string fragment, string icon)
        => new(text, $"/navigation#{fragment}", icon, MatchMode: NavigationMatchMode.Hash);
}
