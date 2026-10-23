using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zoorce.Web.UI.Abstractions.Navigation;
using Zoorce.Web.UI.Abstractions.Notifications;
using Zoorce.Web.UI.Abstractions.UserContext;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Foundation.Notifications;
using Zoorce.Web.UI.Foundation.Progress;
using Zoorce.Web.UI.Foundation.Skeleton;
using Zoorce.Web.UI.Foundation.WaitDialog;
using Zoorce.Web.UI.Layout;
using Zoorce.Web.UI.Layout.Components;
using Zoorce.Web.UI.Theming;

namespace Zoorce.Web.UI.Tests;

/// <summary>Exercises reusable shell, theme, and asynchronous-feedback component boundaries.</summary>
public sealed class ShellThemeAndFoundationTests
{
    [Fact]
    public void Foundation_view_components_reject_missing_models_and_return_view_results()
    {
        Assert.Throws<ArgumentNullException>(() => new ZoorceProgressViewComponent().Invoke(null!));
        Assert.Throws<ArgumentNullException>(() => new ZoorceSkeletonViewComponent().Invoke(null!));
        Assert.Throws<ArgumentNullException>(() => new ZoorceWaitDialogViewComponent().Invoke(null!));

        Assert.IsType<ViewViewComponentResult>(new ZoorceProgressViewComponent().Invoke(new ZoorceProgressOptions()));
        Assert.IsType<ViewViewComponentResult>(new ZoorceSkeletonViewComponent().Invoke(new ZoorceSkeletonOptions()));
        Assert.IsType<ViewViewComponentResult>(new ZoorceWaitDialogViewComponent().Invoke(new ZoorceWaitDialogOptions { Id = "wait" }));
        Assert.IsType<ViewViewComponentResult>(new ZoorceConfirmationDialogViewComponent().Invoke());

        var toasts = new ToastService();
        toasts.Show(new ZoorceToastMessage("Saved", "Your changes are available.", ZoorceToastKind.Success));
        var toastResult = Assert.IsType<ViewViewComponentResult>(new ZoorceToastRegionViewComponent(toasts).Invoke());
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<ZoorceToastMessage>>(toastResult.ViewData!.Model));
        Assert.Empty(toasts.Drain());
    }

    [Fact]
    public void Layout_components_render_host_providers_and_resolve_nested_commands()
    {
        var shell = Options.Create(new ZoorceShellOptions { ApplicationName = "Orbit", BrandMark = "OR" });
        var navigation = new TestNavigationProvider(
        [
            new NavigationItem("Overview", "/overview"),
            new NavigationItem("Parent", Children: [new NavigationItem("Child", "/child")])
        ]);
        var profile = new UserMenuProfile("Ada Lovelace", "Owner");

        Assert.Throws<ArgumentNullException>(() => new ZoorceSidebarNavigationViewComponent(null!, navigation));
        Assert.Throws<ArgumentNullException>(() => new ZoorceTopBarViewComponent(shell, null!));
        Assert.Throws<ArgumentNullException>(() => new ZoorceCommandPaletteViewComponent(null!));

        var sidebar = Assert.IsType<ViewViewComponentResult>(new ZoorceSidebarNavigationViewComponent(shell, navigation).Invoke());
        var sidebarModel = Assert.IsType<ZoorceSidebarNavigationModel>(sidebar.ViewData!.Model);
        Assert.Equal("Orbit", sidebarModel.Shell.ApplicationName);
        Assert.Equal(2, sidebarModel.Items.Count);

        var topBar = Assert.IsType<ViewViewComponentResult>(new ZoorceTopBarViewComponent(shell, new TestUserMenuProvider(profile)).Invoke("  Release  "));
        var topBarModel = Assert.IsType<ZoorceTopBarModel>(topBar.ViewData!.Model);
        Assert.Equal("Release", topBarModel.Title);
        Assert.Equal("A", topBarModel.Initials);

        var palette = Assert.IsType<ViewViewComponentResult>(new ZoorceCommandPaletteViewComponent(navigation).Invoke());
        var leaves = Assert.IsAssignableFrom<IReadOnlyList<NavigationItem>>(palette.ViewData!.Model);
        Assert.Equal(["Overview", "Child"], leaves.Select(item => item.Text));
    }

    [Fact]
    public void Runtime_theming_normalizes_tokens_notifies_subscribers_and_registers_once()
    {
        var service = new ZoorceThemeService(new ZoorceThemeDefinition { Name = " initial ", Tokens = new Dictionary<string, string> { [" --z-primary "] = " #123 ", ["bad"] = "ignored" } });
        ZoorceThemeDefinition? observed = null;
        service.ThemeChanged += (_, theme) => observed = theme;

        service.Apply(new ZoorceThemeDefinition { Name = " next ", Tokens = new Dictionary<string, string> { ["--z-canvas"] = " #010203 " } });

        Assert.Equal("next", service.Current.Name);
        Assert.Equal("#010203", service.Current.Tokens["--z-canvas"]);
        Assert.Same(service.Current, observed);
        Assert.Throws<ArgumentException>(() => new ZoorceThemeDefinition { Name = " " }.Normalize());
        Assert.Throws<ArgumentNullException>(() => service.Apply(null!));

        var services = new ServiceCollection();
        services.AddZoorceTheming(new ZoorceThemeDefinition { Name = "one" });
        services.AddZoorceTheming(_ => new ZoorceThemeDefinition { Name = "two" });
        var resolved = services.BuildServiceProvider().GetRequiredService<IZoorceThemeService>();
        Assert.Equal("one", resolved.Current.Name);
        Assert.Throws<ArgumentNullException>(() => Zoorce.Web.UI.Theming.ServiceCollectionExtensions.AddZoorceTheming(null!, new ZoorceThemeDefinition()));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddZoorceTheming((Func<IServiceProvider, ZoorceThemeDefinition>)null!));

        var catalog = new ZoorceThemeCatalogOptions().UsePreset(ZoorceThemePreset.Amethyst);
        Assert.Contains("data-zoorce-theme", catalog.ToCssRules());
        Assert.Equal("dark", catalog.CreateDefinition(ZoorceThemeMode.Dark).Name);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceThemeCatalogOptions().UsePreset((ZoorceThemePreset)999));
    }

    private sealed class TestNavigationProvider(IReadOnlyList<NavigationItem> items) : INavigationProvider
    {
        public IReadOnlyList<NavigationItem> GetItems() => items;
    }

    private sealed class TestUserMenuProvider(UserMenuProfile? profile) : IUserMenuProvider
    {
        public UserMenuProfile? GetCurrentUser() => profile;
    }
}
