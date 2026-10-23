using Microsoft.Extensions.DependencyInjection;
using Zoorce.Web.UI.Abstractions.Data;
using Zoorce.Web.UI.Data;
using Zoorce.Web.UI.Realtime.SignalR;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies the independent, transport-neutral data lifecycle.</summary>
public sealed class DataRuntimeTests
{
    [Fact]
    public async Task Controller_publishes_loading_success_empty_and_error_states()
    {
        var source = new ScriptedSource<int>(
            (_, _, _) => Task.FromResult(new ZoorceDataPage<int>([7, 9], 2)),
            (_, _, _) => Task.FromResult(ZoorceDataPage<int>.Empty()),
            (_, _, _) => Task.FromException<ZoorceDataPage<int>>(new InvalidOperationException("unavailable")));
        await using var controller = new ZoorceDataController<int>(source);
        var states = new List<ZoorceDataLoadState>();
        controller.SnapshotChanged += (_, snapshot) => states.Add(snapshot.State);

        var first = await controller.LoadAsync(new ZoorceDataQuery("search"), new ZoorcePageRequest { Page = 2, PageSize = 10 });
        Assert.Equal(ZoorceDataLoadState.Success, first.State);
        Assert.Equal([7, 9], first.Page!.Items);
        Assert.Equal(1, first.Progress);

        var empty = await controller.LoadAsync();
        Assert.Equal(ZoorceDataLoadState.Empty, empty.State);
        Assert.Empty(empty.Page!.Items);

        var error = await controller.LoadAsync();
        Assert.Equal(ZoorceDataLoadState.Error, error.State);
        Assert.IsType<InvalidOperationException>(error.Error);
        Assert.Contains(ZoorceDataLoadState.Loading, states);
        Assert.Contains(ZoorceDataLoadState.Refreshing, states);
    }

    [Fact]
    public async Task Invalidation_refreshes_only_its_own_scope_and_manual_events_do_not_reload()
    {
        var bus = new ZoorceDataInvalidationBus();
        var source = new ScriptedSource<string>((_, _, _) => Task.FromResult(new ZoorceDataPage<string>(["ready"])));
        await using var controller = new ZoorceDataController<string>(source, bus, "orders");
        await controller.LoadAsync();
        Assert.Equal(1, source.RequestCount);

        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "people" });
        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "orders", Strategy = ZoorceDataRefreshStrategy.Manual });
        Assert.Equal(1, source.RequestCount);

        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = " orders ", Strategy = ZoorceDataRefreshStrategy.Refresh });
        Assert.Equal(2, source.RequestCount);
    }

    [Fact]
    public async Task Bus_normalizes_scopes_honors_disposal_and_propagates_cancellation()
    {
        var bus = new ZoorceDataInvalidationBus();
        var received = new List<ZoorceDataInvalidation>();
        using var subscription = bus.Subscribe(" activity ", (message, _) =>
        {
            received.Add(message);
            return ValueTask.CompletedTask;
        });

        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "activity", CorrelationId = " trace " });
        Assert.Single(received);
        Assert.Equal("trace", received[0].CorrelationId);
        subscription.Dispose();
        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "activity" });
        Assert.Single(received);

        using var activeSubscription = bus.Subscribe("activity", (_, _) => ValueTask.CompletedTask);
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "activity" }, canceled.Token));
    }

    [Fact]
    public async Task Controller_cancels_a_superseded_request_and_becomes_disposed()
    {
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new ScriptedSource<int>(async (_, _, cancellationToken) =>
        {
            firstStarted.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return ZoorceDataPage<int>.Empty();
        }, (_, _, _) => Task.FromResult(new ZoorceDataPage<int>([42])));
        await using var controller = new ZoorceDataController<int>(source);
        var first = controller.LoadAsync();
        await firstStarted.Task;
        var second = await controller.LoadAsync();
        await first;
        Assert.Equal(ZoorceDataLoadState.Success, second.State);
        Assert.Equal([42], second.Page!.Items);

        await controller.DisposeAsync();
        Assert.Equal(ZoorceDataLoadState.Disposed, controller.Snapshot.State);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => controller.LoadAsync());
    }

    [Fact]
    public void Binding_loading_and_realtime_options_are_normalized_without_a_transport_dependency()
    {
        var binding = new ZoorceDataBindingOptions
        {
            Source = " /api/orders ",
            InvalidationScope = " orders ",
            RefreshStrategy = (ZoorceDataRefreshStrategy)999,
            Loading = new ZoorceLoadingOptions { DisplayDelay = TimeSpan.FromMilliseconds(-1), InitialLoadVisual = (ZoorceLoadVisual)999 }
        }.Normalize();
        Assert.Equal("/api/orders", binding.Source);
        Assert.Equal("orders", binding.InvalidationScope);
        Assert.Equal(ZoorceDataRefreshStrategy.Refresh, binding.RefreshStrategy);
        Assert.True(binding.Loading.Enabled);
        Assert.Equal(TimeSpan.Zero, binding.Loading.DisplayDelay);

        var signalR = new ZoorceSignalRRealtimeOptions { HubUrl = " /hubs/orders ", EventName = " changed " }.Normalize();
        Assert.Equal("/hubs/orders", signalR.HubUrl);
        Assert.Equal("changed", signalR.EventName);
        Assert.Throws<ArgumentException>(() => new ZoorceSignalRRealtimeOptions().Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceDataInvalidation { Scope = " " }.Normalize());
    }

    [Fact]
    public void Data_registration_preserves_an_existing_host_bus()
    {
        var services = new ServiceCollection();
        var existing = new TestInvalidationBus();
        services.AddSingleton<IZoorceDataInvalidationBus>(existing);
        services.AddZoorceData();
        using var provider = services.BuildServiceProvider();
        Assert.Same(existing, provider.GetRequiredService<IZoorceDataInvalidationBus>());
    }

    private sealed class ScriptedSource<T>(params Func<ZoorceDataQuery, ZoorcePageRequest, CancellationToken, Task<ZoorceDataPage<T>>>[] responses) : IZoorceDataSource<T>
    {
        private readonly Queue<Func<ZoorceDataQuery, ZoorcePageRequest, CancellationToken, Task<ZoorceDataPage<T>>>> _responses = new(responses);
        public int RequestCount { get; private set; }

        public Task<ZoorceDataPage<T>> QueryAsync(ZoorceDataQuery query, ZoorcePageRequest pageRequest, CancellationToken cancellationToken = default)
        {
            RequestCount++;
            return _responses.Dequeue()(query, pageRequest, cancellationToken);
        }
    }

    private sealed class TestInvalidationBus : IZoorceDataInvalidationBus
    {
        public ValueTask PublishAsync(ZoorceDataInvalidation invalidation, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public IDisposable Subscribe(string scope, Func<ZoorceDataInvalidation, CancellationToken, ValueTask> handler) => new Disposable();
        private sealed class Disposable : IDisposable { public void Dispose() { } }
    }
}
