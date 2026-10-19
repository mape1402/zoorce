# Component catalog

Zoorce provides a dark-first, semantic-token UI foundation for Razor Pages. The sample project demonstrates each pattern in a single running application.

## Shell and navigation

- Application shell, responsive sidebar, topbar, product brand and user profile surface.
- Desktop sidebar that can hide completely, preserves the visitor preference, and can be configured as fixed instead.
- Navigation density from `Comfortable` through `Dense`, plus an independent, styled scrollbar for long menus.
- Recursive grouped navigation, single current-page state on the final leaf, keyboard disclosure controls, mobile overlay, skip link and quick command palette.
- Light/Dark switcher, with colors injected through `ZoerceWebUiOptions` in dependency injection.

Navigation entries come from the host application's `INavigationProvider`; Zoorce never hard-codes product sections. `NavigationItem.Children` may nest indefinitely, but three visible levels is the recommended information-architecture limit. Use `NavigationMatchMode.Exact` for a single route, `Prefix` for a route branch, `Hash` for a one-page section, `None` for a passive item, or `Auto` for the default behavior. `ActiveUrls` can add aliases without changing the visible target.

The sample’s density selector is a live visual preview. Production defaults continue to come from `ZoerceWebUiOptions.NavigationDensity` during service registration.

## Actions and feedback

- Primary, secondary, ghost, danger, icon and disabled button treatments.
- Client and server-backed toasts via `IToastService`.
- Confirmation dialog, modal, drawer, tooltip, skeleton loader and empty state.

## Encapsulated data components

`ZoorceDataGrid` and `ZoorceChart` are Razor View Components. Every rendered instance has its own root element, config payload, request lifecycle and optional realtime subscription. They can live together on the same document without shared selectors or a page-level coordinator. Server-rendered rows and SVG provide a usable first paint; JavaScript only enhances each individual root.

The host owns the backend implementation through transport-neutral contracts in `Zoerce.WebUI.Abstractions.Data`:

- `IZoorceDataSource<T>` receives a `ZoorceDataQuery` and `ZoorcePageRequest`, then returns `ZoorceDataPage<T>`.
- `ZoorceDataChange<T>` describes optional SignalR mutations (`Upsert`, `Remove`, or `Invalidate`).
- REST uses the same envelope in JSON. A grid needs a URL that returns `{ items, totalCount, hasMore, nextCursor }`.

Use `ZoorceDataGridOptions.LoadMode` to select `Pagination` (previous/next controls) or `InfiniteScroll` (intersection observer plus a keyboard-accessible fallback). Both send the same query and page parameters; the difference is entirely presentational. `Virtualize` bounds appended DOM rows for long streams.

```cshtml
@await Component.InvokeAsync("ZoorceDataGrid", new ZoorceDataGridOptions
{
    Id = "orders",
    AriaLabel = "Orders",
    SourceUrl = "/api/orders",
    LoadMode = ZoorceDataLoadMode.InfiniteScroll,
    RealtimeHubUrl = "/hubs/orders",
    RealtimeEventName = "order-change",
    Columns = [new("number", "Order"), new("status", "Status")]
})
```

`ZoorceChart` renders Line, Area, Bar, Donut and Sparkline presentations with native SVG—there is no charting library dependency. Charts can be static, REST-backed, SignalR-backed, or all three. Its data contract is a collection of named series composed of `ZoorceChartPoint` values.

SignalR is deliberately an opt-in adapter rather than a global shell dependency. When a component is given both realtime options, load the official SignalR browser client and Zoorce's `zoerce-signalr.js` adapter on the page that uses it. Static and REST-only components require neither script.

## Markdown

`IZoorceMarkdownRenderer` is registered in dependency injection and uses Markdig with advanced Markdown extensions, GitHub-style heading identifiers and raw HTML disabled. Generated heading metadata is available through `ZoorceMarkdownRenderResult.TableOfContents`. The renderer preserves only fragment, application-relative, `./`, `../`, HTTP(S), and `mailto` links; other protocols and protocol-relative URLs are replaced with `#`.

For direct Razor use, add `@addTagHelper *, Zoerce.WebUI` and render an independent semantic surface:

```cshtml
<zoerce-markdown markdown="@Model.Documentation" css-class="z-panel"></zoerce-markdown>
```

## Data and content

- Metric cards, insight cards, progress indicators, charts, status chips and badges.
- Data grid with search, filter chips, row actions and pagination controls.
- Documentation layout with navigation, table of contents, callouts, code blocks and copy control.

## Forms

- Text, select, combobox-style datalist, date, date range and textarea inputs.
- Hints, validation/error presentation, checkboxes, radios and switches.
- Tabs and tab panels.

All visual colors consume `--z-*` semantic variables emitted by the Razor layout, including canvas, surfaces, code surfaces, overlay, shadow, foreground, and semantic success/warning/danger/info states. A host can brand both themes without editing Zoorce's generated Tailwind CSS.
