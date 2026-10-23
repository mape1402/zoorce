using Microsoft.AspNetCore.SignalR.Client;
using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Realtime.SignalR;

/// <summary>
/// Bridges one SignalR event stream into the transport-neutral Zoorce invalidation bus.
/// </summary>
public sealed class ZoorceSignalRInvalidationSubscription : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly IDisposable _handler;

    private ZoorceSignalRInvalidationSubscription(HubConnection connection, IDisposable handler)
    {
        _connection = connection;
        _handler = handler;
    }

    /// <summary>
    /// Connects to SignalR and forwards matching event payloads to the supplied invalidation bus.
    /// </summary>
    public static async Task<ZoorceSignalRInvalidationSubscription> ConnectAsync(
        ZoorceSignalRRealtimeOptions options,
        IZoorceDataInvalidationBus invalidationBus,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(invalidationBus);
        var normalized = options.Normalize();
        var builder = new HubConnectionBuilder().WithUrl(normalized.HubUrl);
        if (normalized.AutomaticReconnect)
        {
            builder = builder.WithAutomaticReconnect();
        }

        var connection = builder.Build();
        var handler = connection.On<ZoorceDataInvalidation>(normalized.EventName, invalidation =>
            invalidationBus.PublishAsync(invalidation).AsTask());
        await connection.StartAsync(cancellationToken).ConfigureAwait(false);
        return new ZoorceSignalRInvalidationSubscription(connection, handler);
    }

    /// <summary>Stops the connection and releases the SignalR event handler.</summary>
    public async ValueTask DisposeAsync()
    {
        _handler.Dispose();
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
