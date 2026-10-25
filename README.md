# ZoorceCode

[![Build](https://github.com/mape1402/zoorce/actions/workflows/build-and-release.yml/badge.svg?branch=main)](https://github.com/mape1402/zoorce/actions/workflows/build-and-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Zoorce.Web.UI?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Zoorce.Web.UI)
[![Downloads](https://img.shields.io/nuget/dt/Zoorce.Web.UI?logo=nuget&label=downloads)](https://www.nuget.org/packages/Zoorce.Web.UI)
[![Coverage](https://img.shields.io/badge/coverage-%E2%89%A590%25-2ea44f)](docs/testing.md)
[![License](https://img.shields.io/badge/license-pending%20designation-lightgrey)](#license)

<p align="center">
  <img src="assets/zoorce-code-logo.png" alt="ZoorceCode — Powered by Elysium" width="900" />
</p>

Zoorce is an ecosystem of reusable libraries and tools developed by Elysium Coding. This repository provides `Zoorce.Web.UI`, its modular, server-rendered and progressively enhanced UI library for ASP.NET Core Razor Pages. Every component owns its markup, lifecycle and optional data binding; the host keeps ownership of domain state, navigation and transport.

## Highlights

- Typed, isolated components for layout, navigation, forms, feedback, data and workflow surfaces.
- Complete light and dark semantic palettes, customizable at runtime without a full-page rerender.
- Accessible interaction contracts: keyboard navigation, focus management, live feedback and ARIA labels.
- Host-controlled templates and transitions where content or validation belongs to the application.
- A runnable sample that exercises components as an integrated product UI.

## Packages

| Package | Use it for |
| --- | --- |
| `Zoorce.Web.UI` | The aggregate package: the complete component catalog. |
| `Zoorce.Web.UI.Abstractions` | Cross-package contracts for data, invalidation, navigation and user context. |
| `Zoorce.Web.UI.Foundation` | Buttons, feedback, dialogs, drawers, notifications, progress and skeletons. |
| `Zoorce.Web.UI.Layout` | Application shell, sidebar, top bar, command palette and resizable layout regions. |
| `Zoorce.Web.UI.Navigation` | Breadcrumbs, pagination, menus, tabs, trees, steppers and wizards. |
| `Zoorce.Web.UI.Forms` | Typed input controls, date/time pickers and selection patterns. |
| `Zoorce.Web.UI.DataGrid`, `.Charts`, `.Timelines`, `.Diagrams` | Independent data-rich visual components. |
| `Zoorce.Web.UI.Realtime.SignalR` | Optional SignalR-to-invalidation adapter. |

## Getting started

### 1. Install the aggregate package

```bash
dotnet add package Zoorce.Web.UI
```

### 2. Register Zoorce in `Program.cs`

```csharp
builder.Services
    .AddRazorPages()
    .AddZoorceWebUI(shell =>
    {
        shell.ApplicationName = "My product";
        shell.Themes.UsePreset(ZoorceThemePreset.Amethyst);
        shell.Themes.Light.Primary = "#087a57";
        shell.Themes.Dark.Primary = "#20e7a0";
    });

var app = builder.Build();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();
```

### 3. Render a component

```cshtml
@using Zoorce.Web.UI.Foundation.Components

@await Component.InvokeAsync("ZoorceProgressButton", new ZoorceProgressButtonOptions
{
    Id = "publish-release",
    Text = "Publish release",
    Progress = 0,
    Tone = ZoorceProgressButtonTone.Primary
})
```

Use the component option type to own labels, variants, templates, state and behavior. For visual changes, set the semantic palette tokens once on the shell or scope CSS variables to an individual component root.

### Standalone sign-in page

The sign-in entry point is a presentation component, not an authentication provider. Create a Razor Page and opt out of your application shell by selecting `_ZoorceStandaloneLayout`. Configure the button to target your application's authentication challenge endpoint; configure and secure that endpoint in the host application. If the application uses a fallback authorization policy, allow anonymous access to the sign-in page and its challenge endpoint as appropriate, and validate any return URL in the host.

```cshtml
@page "/sign-in"
@using Zoorce.Web.UI.Layout.Components
@{
    Layout = "_ZoorceStandaloneLayout";
    ViewData["Title"] = "Sign in";
}
@await Component.InvokeAsync("ZoorceSignIn", new ZoorceSignInOptions
{
    ApplicationName = "Your application",
    SignInUrl = "/account/challenge",
    Title = "Welcome back.",
    Description = "Sign in to continue."
})
```

Brand mark or logo, optional wordmark image (`WordmarkUrl`), brand subtitle and hero highlights, illustration, headings, descriptions, button label, hint and footer links are configurable through `ZoorceSignInOptions`. A host can set `WordmarkClass` for custom wordmark styling and disable the diagonal light trails with `ShowShootingStars`. Their color can be overridden with `--z-signin-spark-color`. The default animated vector illustration follows the theme's primary color, and animations respect reduced-motion preferences. You can supply an `IllustrationUrl` for custom artwork, although a raster image will not automatically recolor with the theme. The standalone layout applies the configured Zoorce theme without rendering the sidebar or top bar. Use `ViewData["Language"]` to set the document language. The host can optionally set `SignInEnabled` to `false` when it needs a disabled button.

The sample's sign-in button opens the sample home page by default. Set `Sample:SignInUrl` to a host-owned authentication challenge URL to use another destination. Zoorce does not authenticate users or configure an identity provider; the host application owns that integration and its security.

### Valky help companion

Register the tag helpers with `@addTagHelper *, Zoorce.Web.UI` in `_ViewImports.cshtml`, then place Valky wherever help is needed:

```cshtml
<zoorce-valky id="help-companion" state="thinking" open="true" floating="true"
              label="Valky, open or close help" style="--z-valky-size: 180px">
    <strong>Need a hand?</strong>
    <p>Replace this content with your application's help.</p>
</zoorce-valky>
```

The initial `state` is `idle`, `thinking`, `talking`, or `celebrating`. A host can change it at runtime with `element.dataset.zValkyState = 'thinking'`. The native details control owns bubble open/close, including keyboard interaction; its `open` property can also be changed by the host. Bubble content is a Razor template; alternatively use `message` for encoded plain text. `floating` is optional.

Celebration runs once by default, returns to `idle`, and emits a bubbling `zoorce:valky-state-change` event with `detail.state` and `detail.reason` (`celebration-complete`). Set `celebration-loop="true"` to keep celebrating until the host changes the state; at runtime set `element.dataset.zValkyCelebration` to `loop` or `once`. Customize the duration with `--z-valky-celebration-speed` and the one-shot repetition count with `--z-valky-celebration-count`.

Customize `--z-valky-size`, `--z-valky-bubble-background`, `--z-valky-bubble-color`, `--z-valky-bubble-border`, `--z-valky-inset-inline`, `--z-valky-inset-block` and `--z-valky-z-index` per instance. The optional `image-url` and `body-image-url` override the head and headless-body artwork; replacements must match Valky's original 1230 × 1278 layout and layer alignment because the animation uses fixed clipping and facial coordinates. The component loads its own JavaScript module; ensure the library's static assets are served. Animations are disabled when reduced motion is requested, while one-shot celebration still returns to idle. See the **Valky assistant** sample for interactive states.

```css
#publish-release {
  --z-progress-button-active: #0e7490;
  --z-progress-button-complete: #059669;
}
```

## Run the sample

```bash
dotnet run --project samples/Zoorce.Sample --urls http://127.0.0.1:5135
```

Open `http://127.0.0.1:5135` to explore the component gallery, test light/dark themes and exercise interactive controls.

## Quality and release

```bash
npm ci
npm run css:build
dotnet test tests/Zoorce.Web.UI.Tests/Zoorce.Web.UI.Tests.csproj --configuration Release
for project in src/*/*.csproj; do
  dotnet pack "$project" --configuration Release --output artifacts/packages
done
```

The build and release workflow builds the CSS, compiles the solution, enforces the coverage threshold and packages the libraries. A change to `.release` on `main` triggers the corresponding GitHub release and NuGet publication after those checks pass. See [RELEASE.md](RELEASE.md) for the release process and prerequisites.

See [component customization](docs/components.md), [architecture](docs/architecture.md), [testing](docs/testing.md), and [the changelog](CHANGELOG.md).

## License

The first release requires an explicit license designation before publication. The badge intentionally remains pending until the repository owner selects and adds the license text.
