# Zoorce

Reusable web shell and UI components for ASP.NET Core Razor Pages.

## Repository layout

- `src/Zoerce.WebUI.Abstractions` contains contracts shared by hosts and UI packages.
- `src/Zoerce.WebUI` is the Razor Class Library distributed as the `Zoerce.WebUI` package.
- `samples/Zoerce.WebUI.Sample` is a Razor Pages host used to exercise the package locally.
- `tests/Zoerce.WebUI.Tests` contains automated tests.
- `docs` holds architecture and usage documentation.

The first implementation milestone is a KnOwl-inspired application shell: configurable sidebar, topbar, theme, notifications, cards, and data grids.

Hosts customize the product name, brand mark, starting mode, and every Dark/Light semantic palette token through `builder.Services.AddRazorPages().AddZoerceWebUI(...)`.

```csharp
builder.Services.AddRazorPages().AddZoerceWebUI(options =>
{
    options.ApplicationName = "My product";
    options.DefaultTheme = ZoorceThemeMode.Dark;
    options.SidebarMode = ZoorceSidebarMode.Collapsible;
    options.PersistSidebarState = true;
    options.NavigationDensity = ZoorceNavigationDensity.Compact;
    options.Dark.Primary = "#20e7a0";
    options.Light.Primary = "#087f5b";
});
```

The host supplies sidebar entries through `INavigationProvider`. `NavigationItem.Children` supports a recursive navigation tree, while only the final navigable item receives the active state. Each item owns its own URL and current-state strategy: `Exact`, `Prefix`, `Hash`, `None`, or sensible `Auto` matching. Use `NavigationDensity.Comfortable`, `Compact`, or `Dense` to fit the product’s navigation volume. The sample uses real Razor Page routes and hash destinations.

See [the component catalog](docs/components.md) for the reusable shell and UI patterns included in the package.
