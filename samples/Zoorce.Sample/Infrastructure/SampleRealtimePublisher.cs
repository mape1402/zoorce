using Microsoft.AspNetCore.SignalR;
using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Sample.Infrastructure;

public sealed class SampleRealtimePublisher(
    SampleComponentCatalog catalog,
    IHubContext<ComponentActivityHub> hub)
{
    private int chartRevision;
    private int stepperRevision;
    private int timelineRevision;

    /// <summary>
    /// Emits one demonstrative update only when a person explicitly asks for it.
    /// Keeping the sample quiet by default makes layout and data behaviour inspectable.
    /// </summary>
    public async Task<ComponentCatalogItem> PublishAsync(CancellationToken cancellationToken = default)
    {
        var item = catalog.AdvanceActivity();
        await PublishInvalidationAsync("components", ZoorceDataRefreshStrategy.Patch, new ZoorceDataChange<ComponentCatalogItem>(ZoorceDataChangeKind.Upsert, item.Id, item), cancellationToken);
        await PublishInvalidationAsync("adoption", ZoorceDataRefreshStrategy.Replace, SampleChartData.Create(Interlocked.Increment(ref chartRevision)), cancellationToken);
        return item;
    }

    /// <summary>Publishes one isolated workflow snapshot for the stepper demonstration.</summary>
    public async Task<object> PublishStepperAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = SampleStepperData.Create(Interlocked.Increment(ref stepperRevision));
        await PublishInvalidationAsync("release-stepper", ZoorceDataRefreshStrategy.Replace, snapshot, cancellationToken);
        return snapshot;
    }

    /// <summary>Publishes one append-only event for the live timeline demonstration.</summary>
    public async Task<object> PublishTimelineAsync(CancellationToken cancellationToken = default)
    {
        var update = SampleTimelineData.CreateLiveEvent(Interlocked.Increment(ref timelineRevision));
        await PublishInvalidationAsync("timeline-operations", ZoorceDataRefreshStrategy.Patch, update, cancellationToken);
        return update;
    }

    private Task PublishInvalidationAsync(string scope, ZoorceDataRefreshStrategy strategy, object change, CancellationToken cancellationToken)
        => hub.Clients.All.SendAsync("dataInvalidated", new ZoorceDataInvalidation
        {
            Scope = scope,
            Strategy = strategy,
            Change = change
        }, cancellationToken);
}
