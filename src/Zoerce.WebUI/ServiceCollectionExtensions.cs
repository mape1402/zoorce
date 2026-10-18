using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zoerce.WebUI.Abstractions.Navigation;
using Zoerce.WebUI.Navigation;
using Zoerce.WebUI.Options;

namespace Zoerce.WebUI;

/// <summary>
/// Registers services required by the Zoorce Razor Class Library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Zoorce web UI foundation. Hosts can replace the default navigation provider.
    /// </summary>
    public static IServiceCollection AddZoerceWebUI(
        this IServiceCollection services,
        Action<ZoerceWebUiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ZoerceWebUiOptions();
        configure?.Invoke(options);

        services.AddOptions<ZoerceWebUiOptions>()
            .Configure(target => target.ApplicationName = options.ApplicationName);
        services.TryAddScoped<INavigationProvider, EmptyNavigationProvider>();

        return services;
    }
}
