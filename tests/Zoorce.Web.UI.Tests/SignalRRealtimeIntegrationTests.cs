using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Zoorce.Web.UI.Abstractions.Data;
using Zoorce.Web.UI.Data;
using Zoorce.Web.UI.Realtime.SignalR;

namespace Zoorce.Web.UI.Tests;

/// <summary>Validates the optional SignalR adapter against a real local hub and the neutral invalidation bus.</summary>
public sealed class SignalRRealtimeIntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SignalR_adapter_forwards_hub_messages_to_the_matching_data_scope(bool automaticReconnect)
    {
        await using var host = await TestHubHost.StartAsync();
        var bus = new ZoorceDataInvalidationBus();
        var delivered = new TaskCompletionSource<ZoorceDataInvalidation>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var observer = bus.Subscribe("orders", (invalidation, _) =>
        {
            delivered.TrySetResult(invalidation);
            return ValueTask.CompletedTask;
        });

        await using var subscription = await ZoorceSignalRInvalidationSubscription.ConnectAsync(
            new ZoorceSignalRRealtimeOptions
            {
                HubUrl = host.HubUrl,
                EventName = "dataInvalidated",
                AutomaticReconnect = automaticReconnect
            },
            bus);

        var context = host.Application.Services.GetRequiredService<IHubContext<TestInvalidationHub>>();
        await context.Clients.All.SendAsync("dataInvalidated", new ZoorceDataInvalidation { Scope = " orders ", CorrelationId = " signalr " });

        var observed = await delivered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("orders", observed.Scope);
        Assert.Equal("signalr", observed.CorrelationId);
    }

    [Fact]
    public async Task SignalR_adapter_validates_required_dependencies_and_registers_the_neutral_bus()
    {
        var bus = new ZoorceDataInvalidationBus();
        await Assert.ThrowsAsync<ArgumentNullException>(() => ZoorceSignalRInvalidationSubscription.ConnectAsync(null!, bus));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ZoorceSignalRInvalidationSubscription.ConnectAsync(new ZoorceSignalRRealtimeOptions { HubUrl = "http://localhost" }, null!));

        var services = new ServiceCollection();
        services.AddZoorceSignalRRealtime();
        using var provider = services.BuildServiceProvider();
        Assert.IsType<ZoorceDataInvalidationBus>(provider.GetRequiredService<IZoorceDataInvalidationBus>());
        Assert.Throws<ArgumentNullException>(() => Zoorce.Web.UI.Realtime.SignalR.ServiceCollectionExtensions.AddZoorceSignalRRealtime(null!));
    }

    private sealed class TestInvalidationHub : Hub
    {
    }

    private sealed class TestHubHost(WebApplication application, string hubUrl) : IAsyncDisposable
    {
        public WebApplication Application { get; } = application;

        public string HubUrl { get; } = hubUrl;

        public static async Task<TestHubHost> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            builder.Services.AddSignalR();
            var application = builder.Build();
            application.MapHub<TestInvalidationHub>("/hubs/invalidations");
            await application.StartAsync();
            var addresses = application.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
            var address = Assert.Single(addresses ?? []);
            return new TestHubHost(application, $"{address.TrimEnd('/')}/hubs/invalidations");
        }

        public async ValueTask DisposeAsync()
        {
            await Application.StopAsync();
            await Application.DisposeAsync();
        }
    }
}
