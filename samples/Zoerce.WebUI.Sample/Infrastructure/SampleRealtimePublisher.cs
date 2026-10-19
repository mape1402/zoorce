using Microsoft.AspNetCore.SignalR;
using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleRealtimePublisher(
    SampleComponentCatalog catalog,
    IHubContext<ComponentActivityHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);
            var item = catalog.AdvanceActivity();
            await hub.Clients.All.SendAsync("component-change", new ZoorceDataChange<ComponentCatalogItem>(ZoorceDataChangeKind.Upsert, item.Id, item), stoppingToken);
            await hub.Clients.All.SendAsync("adoption-update", SampleChartData.Create(), stoppingToken);
        }
    }
}
