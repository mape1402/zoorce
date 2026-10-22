using Zoerce.WebUI.Components.Timeline;

namespace Zoerce.WebUI.Sample.Infrastructure;

/// <summary>Provides deterministic timeline snapshots for the component gallery.</summary>
public static class SampleTimelineData
{
    public static object CreateOperationsSnapshot() => new { items = Operations() };

    public static object CreateLiveEvent(int revision) => new
    {
        operation = "upsert",
        item = new
        {
            id = $"live-{revision}",
            timestamp = DateTimeOffset.UtcNow,
            title = revision % 2 == 0 ? "Verification checkpoint passed" : "Deployment signal received",
            description = "A live sample event was received without replacing the page or moving your reading position.",
            icon = revision % 2 == 0 ? "✓" : "↗",
            tone = revision % 2 == 0 ? "Success" : "Info",
            state = "Highlighted",
            isRead = false
        }
    };

    public static IReadOnlyList<ZoorceTimelineItem> Operations() =>
    [
        new("ops-open", DateTimeOffset.UtcNow.AddHours(-7), "Incident opened", "The checkout latency alert crossed its escalation threshold.", "!", ZoorceTimelineTone.Danger, ZoorceTimelineState.Error, Actor: new("Avery Cole", "AC", Role: "On-call"), Tags: ["P1", "Payments"], Metadata: new Dictionary<string, string> { ["Region"] = "US-East" }, IsRead: false),
        new("ops-triage", DateTimeOffset.UtcNow.AddHours(-6), "Triage owner assigned", "The incident was assigned to the commerce reliability rotation.", "⌕", ZoorceTimelineTone.Warning, ZoorceTimelineState.Highlighted, Actor: new("Sofia Reyes", "SR", Role: "Reliability"), Tags: ["Escalated"], IsPinned: true, IsRead: false),
        new("ops-mitigation", DateTimeOffset.UtcNow.AddHours(-4), "Mitigation applied", "Traffic was shifted to the healthy checkout pool while analysis continues.", "↗", ZoorceTimelineTone.Info, ZoorceTimelineState.Completed, Actor: new("Mario Perez", "MP", Role: "Delivery"), Tags: ["Mitigation"]),
        new("ops-observe", DateTimeOffset.UtcNow.AddHours(-1), "Monitoring recovery", "Error rate is returning to baseline; the owner will close after the observation window.", "◉", ZoorceTimelineTone.Success, ZoorceTimelineState.Pending, Actor: new("Nora Ellis", "NE", Role: "Observer"), Tags: ["Watching"], Action: new("view-incident", "Open incident", "↗", "/navigation#incidents"))
    ];
}
