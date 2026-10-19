# Architecture

Zoorce is organized as a reusable Razor Class Library rather than a copied layout per application.

`Zoerce.WebUI.Abstractions` owns the host-facing contracts. `Zoerce.WebUI` owns Razor markup, static web assets, styling, and service-registration extensions. Applications consume the package, provide navigation and user context, and retain ownership of their pages and business domain.

This separation keeps the web shell independent from a specific authentication provider, database, or business module.

Each host chains `AddZoerceWebUI(...)` from `AddRazorPages()`; this registers the packaged layout and the dependency-injection services. The host configures its own `Light` and `Dark` palette in that call. The layout emits those values as semantic CSS variables, including foreground, success, warning, danger, information, and data-visualization colors, allowing Tailwind-built components to change brand colors without recompiling Zoorce.

The same options object configures desktop shell behavior through `SidebarMode`, `SidebarCollapsedByDefault`, `PersistSidebarState`, and `NavigationDensity`. On desktop, a collapsible shell hides the navigation panel completely; on mobile it always becomes an overlay. `NavigationDensity` selects `Comfortable`, `Compact`, or `Dense` spacing without requiring a stylesheet fork.

The host owns navigation data through `INavigationProvider`, including target URLs and `NavigationMatchMode`. `NavigationItem.Children` forms a recursive tree, and the packaged renderer generates a unique disclosure relationship for every branch. Only a matching leaf receives the active state; its ancestors open to preserve context without looking selected. The middle area of the fixed-height sidebar owns its own scroll, leaving product identity and system status in place. Although the renderer supports arbitrary depth, product navigation should normally stop at three visible levels and move finer detail to contextual navigation or command search.
