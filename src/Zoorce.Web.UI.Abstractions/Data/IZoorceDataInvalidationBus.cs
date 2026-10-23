namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Publishes and subscribes to transport-neutral data invalidations.
/// </summary>
public interface IZoorceDataInvalidationBus
{
    /// <summary>Publishes an invalidation to all subscribers of its scope.</summary>
    ValueTask PublishAsync(ZoorceDataInvalidation invalidation, CancellationToken cancellationToken = default);

    /// <summary>Subscribes a handler to a named data scope.</summary>
    IDisposable Subscribe(
        string scope,
        Func<ZoorceDataInvalidation, CancellationToken, ValueTask> handler);
}
