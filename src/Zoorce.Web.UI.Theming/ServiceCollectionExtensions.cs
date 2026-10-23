using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zoorce.Web.UI.Theming;

/// <summary>
/// Registers dynamic Zoorce theme services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds a singleton theme service configured with an optional initial theme.</summary>
    public static IServiceCollection AddZoorceTheming(
        this IServiceCollection services,
        ZoorceThemeDefinition? initialTheme = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IZoorceThemeService>(_ => new ZoorceThemeService(initialTheme));
        return services;
    }

    /// <summary>Adds a singleton theme service whose initial value can be resolved from host configuration.</summary>
    public static IServiceCollection AddZoorceTheming(
        this IServiceCollection services,
        Func<IServiceProvider, ZoorceThemeDefinition> initialThemeFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(initialThemeFactory);
        services.TryAddSingleton<IZoorceThemeService>(provider => new ZoorceThemeService(initialThemeFactory(provider)));
        return services;
    }
}
