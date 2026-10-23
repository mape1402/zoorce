using System.Collections.Concurrent;

namespace Zoorce.Sample.Infrastructure;

/// <summary>Retains sample release drafts so the form demonstration performs a real local save.</summary>
public sealed class SampleReleaseDraftStore
{
    private readonly ConcurrentDictionary<string, SampleReleaseDraft> drafts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Saves a draft and returns the persisted snapshot.</summary>
    public SampleReleaseDraft Save(string releaseName, string risk, string owner, int confidence, IReadOnlyList<string> checks)
    {
        var id = $"rel-{Guid.NewGuid():N}"[..12];
        var draft = new SampleReleaseDraft(id, releaseName, risk, owner, confidence, checks, DateTimeOffset.UtcNow);
        drafts[id] = draft;
        return draft;
    }
}

/// <summary>Represents the persisted values created by the release editor demonstration.</summary>
public sealed record SampleReleaseDraft(
    string Id,
    string ReleaseName,
    string Risk,
    string Owner,
    int Confidence,
    IReadOnlyList<string> Checks,
    DateTimeOffset SavedAt);
