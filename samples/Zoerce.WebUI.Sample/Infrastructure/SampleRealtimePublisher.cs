using Microsoft.AspNetCore.SignalR;
using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleRealtimePublisher(
    SampleComponentCatalog catalog,
    IHubContext<ComponentActivityHub> hub)
{
    private int chartRevision;

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
}
