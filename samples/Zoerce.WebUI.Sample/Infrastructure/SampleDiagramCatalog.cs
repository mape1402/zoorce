using Zoerce.WebUI.Components.Diagrams;

namespace Zoerce.WebUI.Sample.Infrastructure;

/// <summary>Provides independent example documents for the diagram gallery.</summary>
public sealed class SampleDiagramCatalog
{
    private readonly Dictionary<string, ZoorceDiagramDocument> documents = new(StringComparer.OrdinalIgnoreCase)
    {
        ["flow"] = new ZoorceDiagramDocument(1,
        [
            Node("start", "start", "Request received", 70, 330, "Event trigger", ZoorceDiagramTone.Success),
            Node("triage", "process", "Triage request", 300, 339, "Classify and enrich", ZoorceDiagramTone.Info),
            Node("decision", "decision", "Ready to ship?", 545, 321, "Policy decision", ZoorceDiagramTone.Warning, style: new(FillColor: "#fff3d6", BorderColor: "#d78a19", TextColor: "#39230a", CornerRadius: 8m)),
            Node("release", "process", "Create release", 805, 178, "Publish change", ZoorceDiagramTone.Primary, new(ZoorceDiagramActionKind.Event, "flow-release"), new(Gradient: new("#7c5cff", "#2aa8ff", 132), TextColor: "#ffffff", BorderColor: "#9b8cff", FontWeight: 800)),
            Node("hold", "process", "Request changes", 805, 492, "Return to owner", ZoorceDiagramTone.Danger),
            Node("done", "end", "Completed", 1080, 169, "Release is visible", ZoorceDiagramTone.Success),
            Node("closed", "end", "Closed", 1080, 483, "Needs more work", ZoorceDiagramTone.Neutral)
        ],
        [
            Edge("edge-start", "start", "triage"), Edge("edge-triage", "triage", "decision"),
            Edge("edge-yes", "decision", "release", "yes", ZoorceDiagramTone.Success, new("#18a878", 2.5m)), Edge("edge-no", "decision", "hold", "no", ZoorceDiagramTone.Danger, new("#de4b69", 2.5m)),
            Edge("edge-release", "release", "done"), Edge("edge-hold", "hold", "closed")
        ], "sample-flow"),
        ["mind"] = new ZoorceDiagramDocument(1,
        [
            Node("north-star", "root", "Product north star", 610, 320, "Reduce time to value", ZoorceDiagramTone.Primary, style: new(Gradient: new("#7c5cff", "#22c8a2", 125), TextColor: "#ffffff", FontSize: 12m, FontWeight: 800)),
            Node("activation", "branch", "Activation", 920, 160, "First useful action", ZoorceDiagramTone.Success),
            Node("retention", "branch", "Retention", 915, 440, "Weekly habit", ZoorceDiagramTone.Info),
            Node("clarity", "topic", "Guided setup", 1170, 88, "Progressive disclosure", ZoorceDiagramTone.Neutral),
            Node("handoff", "topic", "Human handoff", 1170, 235, "Fast expert help", ZoorceDiagramTone.Warning),
            Node("signals", "topic", "Health signals", 1170, 403, "Early risk cues", ZoorceDiagramTone.Neutral),
            Node("ritual", "topic", "Team ritual", 1170, 554, "Monthly review", ZoorceDiagramTone.Neutral),
            Node("research", "note", "Research note", 350, 520, "Interview 12 customers", ZoorceDiagramTone.Info)
        ],
        [
            Edge("mind-activation", "north-star", "activation"), Edge("mind-retention", "north-star", "retention"), Edge("mind-note", "north-star", "research"),
            Edge("mind-clarity", "activation", "clarity"), Edge("mind-handoff", "activation", "handoff"), Edge("mind-signals", "retention", "signals"), Edge("mind-ritual", "retention", "ritual")
        ], "sample-mind"),
        ["org"] = new ZoorceDiagramDocument(1,
        [
            Node("vp", "person", "Mara Chen", 602, 70, "VP, Product", ZoorceDiagramTone.Primary),
            Node("platform", "team", "Platform", 230, 270, "Foundation group", ZoorceDiagramTone.Info),
            Node("experience", "team", "Experience", 600, 270, "Customer journey", ZoorceDiagramTone.Success),
            Node("growth", "team", "Growth", 970, 270, "Acquisition and expansion", ZoorceDiagramTone.Warning),
            Node("architect", "person", "Ilya Patel", 135, 495, "Staff architect", ZoorceDiagramTone.Neutral),
            Node("systems", "person", "Eli Torres", 332, 495, "Engineering manager", ZoorceDiagramTone.Neutral),
            Node("design", "person", "Noa Rivera", 515, 495, "Design manager", ZoorceDiagramTone.Neutral),
            Node("researcher", "person", "Sam Lee", 705, 495, "Research lead", ZoorceDiagramTone.Neutral),
            Node("open-role", "vacancy", "Open role", 1060, 495, "Lifecycle lead", ZoorceDiagramTone.Warning)
        ],
        [
            Edge("org-platform", "vp", "platform"), Edge("org-experience", "vp", "experience"), Edge("org-growth", "vp", "growth"),
            Edge("org-architect", "platform", "architect"), Edge("org-systems", "platform", "systems"), Edge("org-design", "experience", "design"), Edge("org-research", "experience", "researcher"), Edge("org-vacancy", "growth", "open-role")
        ], "sample-org"),
        ["architecture"] = new ZoorceDiagramDocument(1,
        [
            Node("customer", "user", "Customer app", 70, 300, "Web and mobile", ZoorceDiagramTone.Info),
            Node("edge", "boundary", "Edge boundary", 280, 300, "Authentication and routing", ZoorceDiagramTone.Warning),
            Node("api", "service", "Experience API", 520, 300, "BFF service", ZoorceDiagramTone.Primary, new(ZoorceDiagramActionKind.Event, "architecture-api")),
            Node("identity", "external", "Identity provider", 520, 110, "OIDC", ZoorceDiagramTone.Neutral),
            Node("orders", "service", "Order service", 785, 195, "Domain service", ZoorceDiagramTone.Success),
            Node("events", "queue", "Event stream", 785, 430, "Async messages", ZoorceDiagramTone.Warning),
            Node("store", "database", "Orders store", 1080, 195, "Postgres", ZoorceDiagramTone.Info),
            Node("analytics", "service", "Analytics worker", 1080, 430, "Read model", ZoorceDiagramTone.Neutral)
        ],
        [
            Edge("arc-user", "customer", "edge"), Edge("arc-api", "edge", "api"), Edge("arc-auth", "identity", "api", "token", ZoorceDiagramTone.Info),
            Edge("arc-orders", "api", "orders"), Edge("arc-events", "api", "events", "publish", ZoorceDiagramTone.Warning), Edge("arc-store", "orders", "store"), Edge("arc-worker", "events", "analytics"), Edge("arc-read", "analytics", "store", "read", ZoorceDiagramTone.Info)
        ], "sample-architecture")
    };

    /// <summary>Gets an independent copy so every browser interaction starts with a safe document.</summary>
    public ZoorceDiagramDocument Get(string key) => Clone(documents.TryGetValue(key, out var document) ? document : documents["flow"]);

    /// <summary>Persists a validated browser document in the in-memory demo catalog.</summary>
    public ZoorceDiagramDocument Save(string key, ZoorceDiagramDocument document)
    {
        documents[key] = Clone(document);
        return Get(key);
    }

    private static ZoorceDiagramNode Node(string id, string kind, string title, decimal x, decimal y, string description, ZoorceDiagramTone tone, ZoorceDiagramAction? action = null, ZoorceDiagramNodeStyle? style = null)
        => new(id, kind, title, new(x, y), description, tone, false, action, null, style);

    private static ZoorceDiagramEdge Edge(string id, string from, string to, string? label = null, ZoorceDiagramTone tone = ZoorceDiagramTone.Neutral, ZoorceDiagramEdgeStyle? style = null)
        => new(id, from, to, label, tone, null, style);

    private static ZoorceDiagramDocument Clone(ZoorceDiagramDocument document)
        => new(document.SchemaVersion,
            document.NodeList.Select(node => node with { Position = node.Position with { }, Action = node.Action is null ? null : node.Action with { } }).ToArray(),
            document.EdgeList.Select(edge => edge with { }).ToArray(),
            document.Version,
            document.Title,
            document.Description);
}
