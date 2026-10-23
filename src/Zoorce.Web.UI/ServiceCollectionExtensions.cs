using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Zoorce.Web.UI.Abstractions.Navigation;
using Zoorce.Web.UI.Abstractions.Notifications;
using Zoorce.Web.UI.Abstractions.UserContext;
using Zoorce.Web.UI.Layout.Navigation;
using Zoorce.Web.UI.Layout.UserContext;
using Zoorce.Web.UI.Foundation.Notifications;
using Zoorce.Web.UI.Layout;
using Zoorce.Web.UI.Markdown;
using Zoorce.Web.UI.Data;
using Zoorce.Web.UI.Theming;
using Zoorce.Web.UI.Foundation.Progress;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Timelines;
using Zoorce.Web.UI.Charts;
using Zoorce.Web.UI.DataGrid;
using Zoorce.Web.UI.Diagrams;
using Zoorce.Web.UI.Forms;
using Zoorce.Web.UI.Navigation.Stepper;

namespace Zoorce.Web.UI;

/// <summary>
/// Registers services required by the Zoorce Razor Class Library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Zoorce's Razor views and its dependency-injection services to a Razor Pages or MVC application.
    /// </summary>
    /// <param name="builder">The MVC builder returned by <c>AddRazorPages</c> or <c>AddControllersWithViews</c>.</param>
    /// <param name="configure">An optional configuration delegate for the Zoorce shell and theme.</param>
    /// <returns>The supplied MVC builder for fluent application setup.</returns>
    public static IMvcBuilder AddZoorceWebUI(
        this IMvcBuilder builder,
        Action<ZoorceShellOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddApplicationPart(typeof(ZoorceShellOptions).Assembly);
        builder.AddApplicationPart(typeof(ZoorceProgressViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceButtonViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceTimelineViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceChartViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceDataGridViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceDiagramViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceTextInputViewComponent).Assembly);
        builder.AddApplicationPart(typeof(ZoorceStepperViewComponent).Assembly);
        builder.Services.AddZoorceWebUI(configure);
        return builder;
    }

    /// <summary>
    /// Adds the Zoorce web UI foundation. Hosts can replace the default navigation provider.
    /// </summary>
    public static IServiceCollection AddZoorceWebUI(
        this IServiceCollection services,
        Action<ZoorceShellOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<ZoorceShellOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddScoped<INavigationProvider, EmptyNavigationProvider>();
        services.TryAddScoped<IUserMenuProvider, EmptyUserMenuProvider>();
        services.TryAddScoped<IToastService, ToastService>();
        services.TryAddSingleton<IZoorceMarkdownRenderer, ZoorceMarkdownRenderer>();
        services.AddZoorceData();
        services.AddZoorceTheming(provider =>
        {
            var shell = provider.GetRequiredService<IOptions<ZoorceShellOptions>>().Value;
            return shell.Themes.CreateDefinition(shell.DefaultTheme);
        });

        return services;
    }
}
