using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zoerce.WebUI;
using Zoerce.WebUI.Abstractions.Navigation;
using Zoerce.WebUI.Abstractions.Notifications;
using Zoerce.WebUI.Abstractions.UserContext;
using Zoerce.WebUI.Options;

namespace Zoerce.WebUI.Tests;

public sealed class ThemeAndServiceTests
{
    [Fact]
    public void Palette_defaults_emit_all_semantic_variables()
    {
        var options = new ZoerceWebUiOptions();

        var rules = options.ToCssThemeRules();

        Assert.Contains("data-zoerce-theme=\"light\"", rules);
        Assert.Contains("data-zoerce-theme=\"dark\"", rules);
        Assert.Contains("--z-canvas:#f6f8f7;", rules);
        Assert.Contains("--z-primary:#20e7a0;", rules);
        Assert.Contains("--z-primary-foreground:#ffffff;", rules);
        Assert.Contains("--z-warning:#8a5a00;", rules);
        Assert.Contains("--z-danger:#ff7586;", rules);
        Assert.Contains("--z-data-secondary:#a18bff;", rules);
        Assert.Contains("--z-sidebar:#07120f;", rules);
    }

    [Fact]
    public void Palette_configuration_is_retained_by_options()
    {
        var palette = ZoorceThemePaletteOptions.CreateDark();
        palette.Primary = "#a855f7";
        palette.Glow = "rgba(168, 85, 247, 0.30)";

        var variables = palette.ToCssVariables();

        Assert.Contains("--z-primary:#a855f7;", variables);
        Assert.Contains("--z-glow:rgba(168, 85, 247, 0.30);", variables);
        Assert.Contains("--z-text:#edfff7;", variables);
    }

    [Fact]
    public void AddZoerceWebUi_registers_defaults_and_honors_configuration()
    {
        var services = new ServiceCollection();

        var result = services.AddZoerceWebUI(options =>
        {
            options.ApplicationName = "Atlas";
            options.BrandMark = "A";
            options.DefaultTheme = ZoorceThemeMode.Light;
            options.SidebarMode = ZoorceSidebarMode.Fixed;
            options.SidebarCollapsedByDefault = true;
            options.PersistSidebarState = false;
            options.NavigationDensity = ZoorceNavigationDensity.Dense;
            options.Light.Primary = "#7c3aed";
        });

        Assert.Same(services, result);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var configured = scope.ServiceProvider.GetRequiredService<IOptions<ZoerceWebUiOptions>>().Value;

        Assert.Equal("Atlas", configured.ApplicationName);
        Assert.Equal("A", configured.BrandMark);
        Assert.Equal(ZoorceThemeMode.Light, configured.DefaultTheme);
        Assert.Equal("#7c3aed", configured.Light.Primary);
        Assert.Equal(ZoorceSidebarMode.Fixed, configured.SidebarMode);
        Assert.True(configured.SidebarCollapsedByDefault);
        Assert.False(configured.PersistSidebarState);
        Assert.Equal(ZoorceNavigationDensity.Dense, configured.NavigationDensity);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<INavigationProvider>().GetItems());
        Assert.Null(scope.ServiceProvider.GetRequiredService<IUserMenuProvider>().GetCurrentUser());
    }

    [Fact]
    public void AddZoerceWebUi_preserves_host_navigation_and_user_providers()
    {
        var services = new ServiceCollection();
        services.AddScoped<INavigationProvider, TestNavigationProvider>();
        services.AddScoped<IUserMenuProvider, TestUserMenuProvider>();

        services.AddZoerceWebUI();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Equal("Host", scope.ServiceProvider.GetRequiredService<INavigationProvider>().GetItems().Single().Text);
        Assert.Equal("Ada Lovelace", scope.ServiceProvider.GetRequiredService<IUserMenuProvider>().GetCurrentUser()!.DisplayName);
    }

    [Fact]
    public void Sidebar_options_default_to_a_persistent_collapsible_shell()
    {
        var options = new ZoerceWebUiOptions();

        Assert.Equal(ZoorceSidebarMode.Collapsible, options.SidebarMode);
        Assert.False(options.SidebarCollapsedByDefault);
        Assert.True(options.PersistSidebarState);
        Assert.Equal(ZoorceNavigationDensity.Compact, options.NavigationDensity);
    }

    [Fact]
    public void AddZoerceWebUi_rejects_a_null_service_collection()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddZoerceWebUI((IServiceCollection)null!));
    }

    [Fact]
    public void AddZoerceWebUi_chained_from_razor_pages_registers_the_package_views()
    {
        var services = new ServiceCollection();
        var mvcBuilder = services.AddRazorPages();

        var result = mvcBuilder.AddZoerceWebUI();

        Assert.Same(mvcBuilder, result);
        using var provider = services.BuildServiceProvider();
        var parts = provider.GetRequiredService<Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager>();
        Assert.Contains(parts.ApplicationParts, part => part.Name == typeof(ZoerceWebUiOptions).Assembly.GetName().Name);
    }

    [Fact]
    public void AddZoerceWebUi_rejects_a_null_mvc_builder()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddZoerceWebUI((IMvcBuilder)null!));
    }

    [Fact]
    public void Toast_service_drains_messages_in_order()
    {
        var services = new ServiceCollection();
        services.AddZoerceWebUI();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var toasts = scope.ServiceProvider.GetRequiredService<IToastService>();

        toasts.Show(new ZoorceToastMessage("Saved", "The draft is safe.", ZoorceToastKind.Success));
        toasts.Show(new ZoorceToastMessage("Review", "One item needs attention.", ZoorceToastKind.Warning));

        var pending = toasts.Drain();

        Assert.Collection(
            pending,
            toast => Assert.Equal("Saved", toast.Title),
            toast => Assert.Equal("Review", toast.Title));
        Assert.Empty(toasts.Drain());
    }

    [Fact]
    public void Toast_service_rejects_a_null_message()
    {
        var services = new ServiceCollection();
        services.AddZoerceWebUI();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Throws<ArgumentNullException>(() => scope.ServiceProvider.GetRequiredService<IToastService>().Show(null!));
    }

    [Fact]
    public void Contract_records_expose_every_configured_value()
    {
        var child = new NavigationItem("Child", "/child", "C", MatchMode: NavigationMatchMode.Prefix);
        var parent = new NavigationItem("Parent", "/parent", "P", [child], NavigationMatchMode.Exact, ["/parent", "/parent/overview"]);
        var root = new NavigationItem("Root", "/");
        var empty = new NavigationItem("Empty");
        var action = new UserMenuAction("Profile", "/profile", "◌");
        var profile = new UserMenuProfile("Ada Lovelace", "ada@example.test", "https://example.test/ada.png", [action]);
        var toast = new ZoorceToastMessage("Ready", "The sample is ready.", ZoorceToastKind.Danger, 1200);

        Assert.Equal("/parent", parent.Url);
        Assert.Equal("P", parent.Icon);
        Assert.Same(child, parent.Children!.Single());
        Assert.Equal(NavigationMatchMode.Exact, parent.MatchMode);
        Assert.Equal(["/parent", "/parent/overview"], parent.GetActiveUrls());
        Assert.Equal(NavigationMatchMode.Prefix, child.MatchMode);
        Assert.Equal(["/"], root.GetActiveUrls());
        Assert.Empty(empty.GetActiveUrls());
        Assert.Equal("Profile", action.Text);
        Assert.Equal("/profile", action.Url);
        Assert.Equal("◌", action.Icon);
        Assert.Equal("ada@example.test", profile.Email);
        Assert.Equal("https://example.test/ada.png", profile.AvatarUrl);
        Assert.Same(action, profile.Actions!.Single());
        Assert.Equal("The sample is ready.", toast.Message);
        Assert.Equal(ZoorceToastKind.Danger, toast.Kind);
        Assert.Equal(1200, toast.DurationMilliseconds);
    }

    private sealed class TestNavigationProvider : INavigationProvider
    {
        public IReadOnlyList<NavigationItem> GetItems() => [new("Host", "/host")];
    }

    private sealed class TestUserMenuProvider : IUserMenuProvider
    {
        public UserMenuProfile GetCurrentUser() => new("Ada Lovelace");
    }
}
