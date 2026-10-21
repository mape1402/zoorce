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

For isolated local integrations, a grid root also accepts `zoerce:grid:insert` with an `{ item }` detail and `zoerce:grid:refresh`. These events update only that grid instance; they are useful for a host action, a modal workflow, or a SignalR adapter without coupling the rest of the document.

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

`ZoorceChart` renders native SVG—there is no charting library dependency. Charts can be static, REST-backed, SignalR-backed, or all three. Its data contract is a collection of named series composed of `ZoorceChartPoint` values, and every root owns its own render loop, ResizeObserver, tooltip, hidden-series state and realtime subscription.

The visual catalog includes Line, Area, Sparkline, vertical Bar, HorizontalBar, Donut, Gauge, Funnel, Heatmap, Scatter, Bubble, Bullet, Histogram and Waterfall. Vertical bars support `Grouped`, `Stacked`, and `Stacked100` composition; line-style charts support `Straight`, `Smooth`, and `Step` interpolation.

`ZoorceChartPoint` keeps the simple `Label` / `Value` pair for normal categorical charts and exposes optional `X`, `Low`, `High`, `Size`, and `Group` dimensions for continuous, ranged, bubble and matrix data. `ZoorceChartSeries.Color` can override a series color; otherwise the component uses the analytical `DataPrimary` through `DataQuinary` palette from the configured theme. Those tokens are intentionally independent from `Primary`, so a product brand does not turn every data series into the same color.

Use `ShowLegend`, `ShowGrid`, `ShowAxes`, and `ShowTooltip` to tune presentation per instance. `MinimumValue`, `MaximumValue`, and `TargetValue` make bounded metrics, gauges and bullet charts deterministic. The legend is keyboard accessible and toggles only its own component instance. SVG paths use CSS-pixel view boxes and non-scaling strokes, avoiding blurred lines at normal browser zoom levels.

A chart root can receive `zoerce:chart:replace` with `{ series }` for an instance-only data update, or `zoerce:chart:refresh` to reload its configured source. Both preserve the rest of the host page and make local, REST, and realtime demos follow the same boundary.

```cshtml
@await Component.InvokeAsync("ZoorceChart", new ZoorceChartOptions
{
    Id = "release-confidence",
    Title = "Release confidence",
    Summary = "Confidence against the deployment target",
    Kind = ZoorceChartKind.Bullet,
    MinimumValue = 0,
    MaximumValue = 100,
    TargetValue = 92,
    ValueSuffix = "%",
    ShowLegend = false,
    Series = [new ZoorceChartSeries("Confidence", [new("Current", 87)])]
})
```

SignalR is deliberately an opt-in adapter rather than a global shell dependency. When a component is given both realtime options, load the official SignalR browser client and Zoorce's `zoerce-signalr.js` adapter on the page that uses it. Static and REST-only components require neither script.

## Workflow steppers

`ZoorceStepper` is a Razor View Component for server-rendered progress, navigation and wizard flows. Every instance carries its own state payload, keyboard handling, optional REST refresh, and optional SignalR subscription; an update changes only that root and never reconstructs the surrounding page.

Choose `Rail`, `Segmented`, `Sidebar`, `Workflow`, `NumberedRail`, `FlatSegments`, `WizardHeader`, or `MinimalRail` through `ZoorceStepperVariant`. `NumberedRail` uses square ordered milestones; `FlatSegments` is a rectangular, non-chevron stage bar; `WizardHeader` binds previous/next actions to a named current-stage header; and `MinimalRail` reveals only the active label above a compact marker rail. `Orientation` can be `Horizontal`, `Vertical`, or `Auto`; auto rails switch to a vertical arrangement when their own container becomes narrow. The normalizer deliberately keeps segmented, flat-segment, and header-wizard controls horizontal, while sidebar journeys stay vertical. `ZoorceStepperInteraction` separates a passive `Progress` display from direct `Navigation` and one-step-at-a-time `Wizard` behavior.

Each `ZoorceStepperStep` has an id, title, optional supporting text or icon, semantic state (`Pending`, `Active`, `Completed`, `Attention`, `Error`, or `Disabled`), optional status text, target URL, panel id, and an optional `Tone` (`Neutral`, `Primary`, `Success`, `Warning`, `Danger`, or `Info`). `AllowCompletedNavigation` and `AllowFutureNavigation` define the basic navigation policy instead of baking product rules into the component.

`Presentation` supplies instance-level visual choices without global CSS overrides: `MarkerStyle` (`Number`, `Icon`, `Check`, `Dot`, or `None`), `ActiveIndicator` (`Ring`, `Halo`, `Solid`, or `Outline`), `ActiveMotion` (`HaloPulse`, `RingPulse`, `MarkerPulse`, or `ConnectorDraw`), `LabelPlacement`, and `ConnectorStyle`. Motion automatically respects the visitor's reduced-motion setting. Semantic colors always come from the injected `Light` and `Dark` palette tokens (`Primary`, `Success`, `Warning`, `Danger`, and `Info`), so changing a host palette in `AddZoerceWebUI` immediately changes every stepper without component-specific color duplication.

`TransitionRules` makes state movement explicit: configure forward movement (`AdjacentOnly`, `AnyEligible`, or `Block`), backward movement (`CompletedOnly`, `AnyEligible`, or `Block`), warning/error behavior, and whether moving backward preserves or resets later steps. For business authorization, add `TransitionUrl`; the component posts a `ZoorceStepperTransitionRequest` and changes only when the endpoint returns an allowed `ZoorceStepperTransitionDecision`. An accepted decision may include a `ZoorceStepperSnapshot`, letting REST or SignalR update an individual step's state, tone, copy, icon, destination, and navigability without rerendering the page.

```cshtml
@await Component.InvokeAsync("ZoorceStepper", new ZoorceStepperOptions
{
    Id = "release-workflow",
    AriaLabel = "Release readiness",
    Variant = ZoorceStepperVariant.Rail,
    Orientation = ZoorceStepperOrientation.Auto,
    Interaction = ZoorceStepperInteraction.Wizard,
    Presentation = new ZoorceStepperPresentation
    {
        MarkerStyle = ZoorceStepperMarkerStyle.Number,
        ActiveIndicator = ZoorceStepperActiveIndicator.Halo,
        ActiveMotion = ZoorceStepperActiveMotion.HaloPulse,
        ConnectorStyle = ZoorceStepperConnectorStyle.Progress
    },
    TransitionRules = new ZoorceStepperTransitionRules
    {
        Forward = ZoorceStepperForwardRule.AdjacentOnly,
        Backward = ZoorceStepperBackwardRule.CompletedOnly,
        Warning = ZoorceStepperWarningRule.RequireAuthorization,
        Error = ZoorceStepperErrorRule.Block,
        OnBackward = ZoorceStepperBackwardEffect.ResetFollowing
    },
    TransitionUrl = "/api/releases/authorize-transition",
    SourceUrl = "/api/releases/current",
    RealtimeHubUrl = "/hubs/releases",
    RealtimeEventName = "release-update",
    Steps =
    [
        new("brief", "Release brief", State: ZoorceStepperState.Completed),
        new("validation", "Validation", State: ZoorceStepperState.Active),
        new("handoff", "Handoff")
    ]
})
```

REST and realtime snapshots use `{ currentStepId, steps: [{ id, state, tone, title, supportingText, statusText, icon, isNavigable, targetUrl }] }`. A transition request contains `stepperId`, `stepId`, `previousStepId`, `reason`, and `isBackward`. `RealtimeTransport` defaults to `Auto` and can explicitly select `WebSockets`, `ServerSentEvents`, or `LongPolling` when a host browser environment has a known transport constraint. The component emits cancelable `zoerce:stepper:request-change` before a local navigation request, followed by `zoerce:stepper:change`; listen on that one component root to run validation or synchronize adjacent content. `zoerce:stepper:loaded`, `zoerce:stepper:realtime`, and `zoerce:stepper:error` provide isolated transport lifecycle hooks. `IZoorceStepperTransitionPolicy` is the host-side contract when an application prefers injecting a policy service behind its own transition endpoint.

## Diagram workspace

`ZoorceDiagram` is a dependency-free diagram surface. It keeps its document and interaction state inside its own component root: it never relies on a global page controller. The camera projects HTML nodes and native SVG connections into screen space rather than scaling a single DOM layer, so text, cards and connection paths remain sharp at browser zoom.

Choose `Flowchart`, `MindMap`, `OrgChart`, or `Architecture` with `ZoorceDiagramProfile`; choose `Viewer`, `ReadOnly`, or `Editor` with `ZoorceDiagramMode`. The editor includes pan, grid snapping, palette insertion, selection, multi-select, drag positioning, keyboard delete, `Ctrl/Cmd+C` and `Ctrl/Cmd+V` node duplication, undo/redo, an inspector, navigator and an interactive overview. Inspector values persist automatically after a brief debounce, while the Save icon remains available to flush immediately. Mind maps and org charts also collapse a branch with a double-click or inspector control.

Nodes use profile-aware SVG geometry rather than one generic card: decisions are spacious diamonds with a compact icon and title-only treatment, starts and ends are capsules, data stores are cylinders, documents have a curved foot, and semantic mind-map, org and architecture nodes retain their own silhouettes. The palette supports both click-to-add and native drag-and-drop, placing a new node under the release point on the canvas. Connections begin directly on any visible node anchor; drag to a target node to snap the preview onto its appropriate anchor, or click an anchor and then a target node. `ZoorceDiagramEdge.FromPort` and `ToPort` accept `Auto`, `Top`, `Right`, `Bottom`, or `Left`, making a JSON document able to preserve intentional routing. Org charts use top/bottom anchors and shared hierarchical bus lines by default, while explicit non-auto anchors retain their direct route. The Focus button elevates one diagram into an escape-key-dismissable full-viewport workspace without using browser fullscreen permissions.

Set `PanMode` to `Background`, `SpaceKey`, `MiddleButton`, or `Disabled` to make viewport movement match the host product. The overview appears only when a diagram warrants it, labels itself clearly, shows the current viewport, and can be clicked or dragged to navigate.

Documents are portable JSON through `ZoorceDiagramDocument`, `ZoorceDiagramNode`, and `ZoorceDiagramEdge`. Flowcharts allow a maximum of one `start` node and block outgoing connections from `end` nodes. Mind maps and org charts require exactly one root and one parent at most for every descendant. The client repeats these structural checks before save so a user gets immediate feedback; hosts should enforce their domain-specific rules at the receiving endpoint too.

`ZoorceDiagramNodeStyle` supplies per-node fill, border, two-stop gradient, text colour, font family, size, weight, corner radius and opacity. `ZoorceDiagramEdgeStyle` supplies a connection colour, width and dashed treatment. The Appearance tab edits those values without changing other nodes; both styles travel in the document JSON. The inspector scrolls independently from the canvas, so appearance controls never alter the diagram viewport. The editor separates parallel flow connections into lanes and keeps each connection's configured stroke through its arrowhead, so decision branches such as `yes` and `no` remain visually unambiguous.

```cshtml
@await Component.InvokeAsync("ZoorceDiagram", new ZoorceDiagramOptions
{
    Id = "release-routing",
    Title = "Release routing",
    Summary = "An editable release decision flow",
    Profile = ZoorceDiagramProfile.Flowchart,
    Mode = ZoorceDiagramMode.Editor,
    Layout = ZoorceDiagramLayout.Flow,
    SourceUrl = "/api/diagrams/release-routing",
    SaveUrl = "/api/diagrams/release-routing",
    Document = new ZoorceDiagramDocument(1,
    [
        new("start", "start", "Request received", new(80, 220)),
        new("review", "process", "Review request", new(350, 220)),
        new("done", "end", "Completed", new(660, 220))
    ],
    [
        new("start-review", "start", "review"),
        new("review-done", "review", "done")
    ])
})
```

Set `SourceUrl` to reload one diagram document and `SaveUrl` to `PUT` its full JSON document. Editor changes autosave after a short debounce; the toolbar save icon and the host `zoerce:diagram:save` event both flush immediately. Its canvas controls retain zoom out, current scale, zoom in and Fit alongside Undo and Redo, while Focus moves the diagram into a full-viewport workspace without changing the user's current zoom or pan position. Provide `RealtimeHubUrl` plus `RealtimeEventName` for an optional SignalR full-document subscription. The component also accepts host-dispatched `zoerce:diagram:replace`, `zoerce:diagram:focus`, `zoerce:diagram:validate`, and `zoerce:diagram:save` events; these act on one root only.

Its emitted events are `zoerce:diagram:nodeclick`, `zoerce:diagram:edgeclick`, `zoerce:diagram:selectionchanged`, `zoerce:diagram:changed`, `zoerce:diagram:validated`, `zoerce:diagram:saved`, `zoerce:diagram:replaced`, and `zoerce:diagram:error`. Node actions are declarative (`Navigate`, `Panel`, `Modal`, `Event`, `Command`, or `Endpoint`): hosts can intercept the cancelable `zoerce:diagram:action` event without exposing raw scripts in a JSON document.

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

All visual colors consume `--z-*` semantic variables emitted by the Razor layout, including canvas, surfaces, code surfaces, overlay, shadow, foreground, and semantic success/warning/danger/info states. Configure `Light.DataPrimary` through `Light.DataQuinary` and their `Dark` equivalents in `AddZoerceWebUI` to establish a product's analytical palette independently from its brand accent. A host can brand both themes without editing Zoorce's generated Tailwind CSS.
