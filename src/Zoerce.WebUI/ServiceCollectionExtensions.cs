using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zoerce.WebUI.Abstractions.Navigation;
using Zoerce.WebUI.Abstractions.Notifications;
using Zoerce.WebUI.Abstractions.UserContext;
using Zoerce.WebUI.Navigation;
using Zoerce.WebUI.Notifications;
using Zoerce.WebUI.Options;
using Zoerce.WebUI.Markdown;

namespace Zoerce.WebUI;

/// <summary>
/// Registers services required by the Zoorce Razor Class Library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Zoerce's Razor views and its dependency-injection services to a Razor Pages or MVC application.
    /// </summary>
    /// <param name="builder">The MVC builder returned by <c>AddRazorPages</c> or <c>AddControllersWithViews</c>.</param>
    /// <param name="configure">An optional configuration delegate for the Zoerce shell and theme.</param>
    /// <returns>The supplied MVC builder for fluent application setup.</returns>
    public static IMvcBuilder AddZoerceWebUI(
        this IMvcBuilder builder,
        Action<ZoerceWebUiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddApplicationPart(typeof(ZoerceWebUiOptions).Assembly);
        builder.Services.AddZoerceWebUI(configure);
        return builder;
    }

    /// <summary>
    /// Adds the Zoorce web UI foundation. Hosts can replace the default navigation provider.
    /// </summary>
    public static IServiceCollection AddZoerceWebUI(
        this IServiceCollection services,
        Action<ZoerceWebUiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<ZoerceWebUiOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddScoped<INavigationProvider, EmptyNavigationProvider>();
        services.TryAddScoped<IUserMenuProvider, EmptyUserMenuProvider>();
        services.TryAddScoped<IToastService, ToastService>();
        services.TryAddSingleton<IZoorceMarkdownRenderer, ZoorceMarkdownRenderer>();

        return services;
    }
}
