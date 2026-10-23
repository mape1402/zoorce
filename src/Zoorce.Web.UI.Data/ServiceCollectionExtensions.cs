using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Data;

/// <summary>
/// Registers transport-neutral Zoorce data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the shared invalidation bus used by independent data-bound components.</summary>
    public static IServiceCollection AddZoorceData(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IZoorceDataInvalidationBus, ZoorceDataInvalidationBus>();
        return services;
    }
}
