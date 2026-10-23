using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Data;

/// <summary>
/// Provides an in-process implementation of the transport-neutral invalidation bus.
/// </summary>
public sealed class ZoorceDataInvalidationBus : IZoorceDataInvalidationBus
{
    private readonly object _sync = new();
    private readonly Dictionary<string, List<Func<ZoorceDataInvalidation, CancellationToken, ValueTask>>> _handlers = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async ValueTask PublishAsync(ZoorceDataInvalidation invalidation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invalidation);
        var normalized = invalidation.Normalize();
        Func<ZoorceDataInvalidation, CancellationToken, ValueTask>[] handlers;

        lock (_sync)
        {
            handlers = _handlers.TryGetValue(normalized.Scope, out var values) ? [.. values] : [];
        }

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await handler(normalized, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe(
        string scope,
        Func<ZoorceDataInvalidation, CancellationToken, ValueTask> handler)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new ArgumentException("A data scope is required.", nameof(scope));
        }

        ArgumentNullException.ThrowIfNull(handler);
        var normalizedScope = scope.Trim();
        lock (_sync)
        {
            if (!_handlers.TryGetValue(normalizedScope, out var handlers))
            {
                handlers = [];
                _handlers[normalizedScope] = handlers;
            }

            handlers.Add(handler);
        }

        return new Subscription(this, normalizedScope, handler);
    }

    private void Unsubscribe(string scope, Func<ZoorceDataInvalidation, CancellationToken, ValueTask> handler)
    {
        lock (_sync)
        {
            if (!_handlers.TryGetValue(scope, out var handlers))
            {
                return;
            }

            handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                _handlers.Remove(scope);
            }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly ZoorceDataInvalidationBus _owner;
        private readonly string _scope;
        private readonly Func<ZoorceDataInvalidation, CancellationToken, ValueTask> _handler;
        private int _disposed;

        public Subscription(
            ZoorceDataInvalidationBus owner,
            string scope,
            Func<ZoorceDataInvalidation, CancellationToken, ValueTask> handler)
        {
            _owner = owner;
            _scope = scope;
            _handler = handler;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _owner.Unsubscribe(_scope, _handler);
            }
        }
    }
}
