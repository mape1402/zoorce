namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Provides a transport-neutral result page for a Zoorce data component.
/// </summary>
public interface IZoorceDataSource<T>
{
    /// <summary>Gets a page that matches the requested data state.</summary>
    Task<ZoorceDataPage<T>> QueryAsync(
        ZoorceDataQuery query,
        ZoorcePageRequest pageRequest,
        CancellationToken cancellationToken = default);
}
