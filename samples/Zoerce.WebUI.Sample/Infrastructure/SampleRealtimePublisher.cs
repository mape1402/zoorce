using Microsoft.AspNetCore.SignalR;
using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Sample.Infrastructure;

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
        await hub.Clients.All.SendAsync("component-change", new ZoorceDataChange<ComponentCatalogItem>(ZoorceDataChangeKind.Upsert, item.Id, item), cancellationToken);
        await hub.Clients.All.SendAsync("adoption-update", SampleChartData.Create(Interlocked.Increment(ref chartRevision)), cancellationToken);
        return item;
    }

    /// <summary>Publishes one isolated workflow snapshot for the stepper demonstration.</summary>
    public async Task<object> PublishStepperAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = SampleStepperData.Create(Interlocked.Increment(ref stepperRevision));
        await hub.Clients.All.SendAsync("release-stepper-update", snapshot, cancellationToken);
        return snapshot;
    }

    /// <summary>Publishes one append-only event for the live timeline demonstration.</summary>
    public async Task<object> PublishTimelineAsync(CancellationToken cancellationToken = default)
    {
        var update = SampleTimelineData.CreateLiveEvent(Interlocked.Increment(ref timelineRevision));
        await hub.Clients.All.SendAsync("timeline-operations-update", update, cancellationToken);
        return update;
    }
}
