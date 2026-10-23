using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Data;

/// <summary>
/// Owns the asynchronous lifecycle of one data-bound component instance.
/// </summary>
/// <typeparam name="T">The item type returned by the configured data source.</typeparam>
public sealed class ZoorceDataController<T> : IAsyncDisposable
{
    private readonly IZoorceDataSource<T> _dataSource;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly IDisposable? _invalidationSubscription;
    private CancellationTokenSource? _activeRequest;
    private ZoorceDataQuery _lastQuery = new();
    private ZoorcePageRequest _lastPageRequest = new();
    private long _requestVersion;
    private bool _disposed;

    /// <summary>
    /// Initializes a controller with one source and an optional invalidation scope.
    /// </summary>
    public ZoorceDataController(
        IZoorceDataSource<T> dataSource,
        IZoorceDataInvalidationBus? invalidationBus = null,
        string? invalidationScope = null)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        if (invalidationBus is not null && !string.IsNullOrWhiteSpace(invalidationScope))
        {
            _invalidationSubscription = invalidationBus.Subscribe(invalidationScope, HandleInvalidationAsync);
        }
    }

    /// <summary>Gets the latest observable state for this controller.</summary>
    public ZoorceDataSnapshot<T> Snapshot { get; private set; } = new();

    /// <summary>Raised whenever <see cref="Snapshot"/> changes.</summary>
    public event EventHandler<ZoorceDataSnapshot<T>>? SnapshotChanged;

    /// <summary>
    /// Requests a page while cancelling any prior request owned by this controller.
    /// </summary>
    public async Task<ZoorceDataSnapshot<T>> LoadAsync(
        ZoorceDataQuery? query = null,
        ZoorcePageRequest? pageRequest = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var effectiveQuery = query ?? new ZoorceDataQuery();
        var effectivePageRequest = pageRequest ?? new ZoorcePageRequest();
        CancellationTokenSource requestSource;
        long version;

        await _stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _activeRequest?.Cancel();
            _activeRequest?.Dispose();
            _activeRequest = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            requestSource = _activeRequest;
            version = ++_requestVersion;
            _lastQuery = effectiveQuery;
            _lastPageRequest = effectivePageRequest;
            Publish(Snapshot with
            {
                State = Snapshot.Page is null ? ZoorceDataLoadState.Loading : ZoorceDataLoadState.Refreshing,
                Error = null,
                Progress = null,
                ChangedAt = DateTimeOffset.UtcNow
            });
        }
        finally
        {
            _stateLock.Release();
        }

        try
        {
            var page = await _dataSource.QueryAsync(effectiveQuery, effectivePageRequest, requestSource.Token).ConfigureAwait(false);
            await ApplyAcceptedResultAsync(version, page, null).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (requestSource.IsCancellationRequested)
        {
            // A newer request owns the visible state; the cancelled result is intentionally ignored.
        }
        catch (Exception exception)
        {
            await ApplyAcceptedResultAsync(version, null, exception).ConfigureAwait(false);
        }

        return Snapshot;
    }

    /// <summary>Loads the last requested query and page again.</summary>
    public Task<ZoorceDataSnapshot<T>> RefreshAsync(CancellationToken cancellationToken = default)
        => LoadAsync(_lastQuery, _lastPageRequest, cancellationToken);

    /// <summary>Disposes active requests and subscriptions owned by this controller.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _invalidationSubscription?.Dispose();
        await _stateLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _activeRequest?.Cancel();
            _activeRequest?.Dispose();
            _activeRequest = null;
            Publish(Snapshot with { State = ZoorceDataLoadState.Disposed, ChangedAt = DateTimeOffset.UtcNow });
        }
        finally
        {
            _stateLock.Release();
            _stateLock.Dispose();
        }
    }

    private async ValueTask HandleInvalidationAsync(ZoorceDataInvalidation invalidation, CancellationToken cancellationToken)
    {
        if (invalidation.Strategy is ZoorceDataRefreshStrategy.Manual)
        {
            return;
        }

        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyAcceptedResultAsync(long version, ZoorceDataPage<T>? page, Exception? error)
    {
        await _stateLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed || version != _requestVersion)
            {
                return;
            }

            Publish(error is null
                ? Snapshot with
                {
                    State = page?.Items.Count > 0 ? ZoorceDataLoadState.Success : ZoorceDataLoadState.Empty,
                    Page = page,
                    Error = null,
                    Progress = 1,
                    ChangedAt = DateTimeOffset.UtcNow
                }
                : Snapshot with
                {
                    State = ZoorceDataLoadState.Error,
                    Error = error,
                    Progress = null,
                    ChangedAt = DateTimeOffset.UtcNow
                });
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private void Publish(ZoorceDataSnapshot<T> snapshot)
    {
        Snapshot = snapshot;
        SnapshotChanged?.Invoke(this, snapshot);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
