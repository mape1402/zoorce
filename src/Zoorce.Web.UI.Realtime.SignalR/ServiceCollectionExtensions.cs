using Microsoft.Extensions.DependencyInjection;
using Zoorce.Web.UI.Data;

namespace Zoorce.Web.UI.Realtime.SignalR;

/// <summary>
/// Registers optional SignalR realtime integration for Zoorce data components.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the transport-neutral bus required by SignalR invalidation subscriptions.</summary>
    public static IServiceCollection AddZoorceSignalRRealtime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddZoorceData();
        return services;
    }
}
