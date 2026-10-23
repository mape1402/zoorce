# Zoorce Web UI

Zoorce is a family of reusable, server-rendered and progressively enhanced ASP.NET Core UI packages. Each control owns its markup, configuration, lifecycle, and optional data binding; the sample application only composes those packages and exercises their behavior.

## Package topology

| Package | Responsibility |
| --- | --- |
| `Zoorce.Web.UI.Abstractions` | Cross-package contracts: data, invalidation, loading, navigation, and user context. |
| `Zoorce.Web.UI.Data` | Transport-neutral data lifecycle, scoped invalidation bus, and browser data runtime. |
| `Zoorce.Web.UI.Theming` | Runtime theme definitions and semantic token application. |
| `Zoorce.Web.UI.Foundation` | Buttons, overlays, progress, skeletons, toasts, and wait dialog. |
| `Zoorce.Web.UI.Layout` | Application shell, sidebar, top bar, command palette, and reusable layout regions. |
| `Zoorce.Web.UI.Navigation` | Navigation patterns and steppers. |
| `Zoorce.Web.UI.Forms` | Individually named form controls and their typed options. |
| `Zoorce.Web.UI.DataGrid`, `.Charts`, `.Timelines`, `.Diagrams` | Independently refreshing data-rich components. |
| `Zoorce.Web.UI.Realtime.SignalR` | Optional SignalR-to-invalidation adapter; no visual component depends on it. |
| `Zoorce.Web.UI` | Convenience aggregate package for hosts that want the complete catalog. |

`samples/Zoorce.Sample` deliberately does not belong to the package family. It is a live host used for browser-level functional verification.

## Host setup

```csharp
builder.Services
    .AddRazorPages()
    .AddZoorceWebUI(shell =>
    {
        shell.ApplicationName = "My product";
        shell.Themes.UsePreset(ZoorceThemePreset.Amethyst);
        shell.Themes.Dark.Primary = "#c4b5fd";
    });

// Optional: only a host that actually receives SignalR pushes adds this adapter.
builder.Services.AddZoorceSignalRRealtime();
```

Every data-capable component accepts a `ZoorceDataBindingOptions`. `Source` is resolved by the host/browser data runtime and `InvalidationScope` causes only the owning root to refresh. REST, GraphQL, local cache, polling, server-sent events, and SignalR can all publish the same `ZoorceDataInvalidation`; components do not know which transport produced it.

```csharp
Binding = new ZoorceDataBindingOptions
{
    Source = "/api/orders",
    InvalidationScope = "orders",
    Loading = new ZoorceLoadingOptions { Enabled = true }
}
```

Loading feedback is enabled by default. A data-bound control may render a skeleton on first load, retain previous content with a refresh progress treatment, or let the host disable either behavior per instance.

## Theming at runtime

The `IZoorceThemeService` applies a normalized `ZoorceThemeDefinition`; the browser theme module updates semantic CSS variables without re-rendering the whole page. All packages consume these variables rather than hard-coded palette values.

```csharp
themeService.Apply(currentTheme with
{
    Primary = "#0ea5e9",
    Canvas = "#07111f"
});
```

Read [architecture](docs/architecture.md), [components](docs/components.md), [migration](docs/migration.md), and [testing](docs/testing.md) for the complete host contract, package-level customization examples, and quality gates.
