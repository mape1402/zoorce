# Architecture

## Composition rules

Each public control is a dedicated View Component with a typed options contract. Shared rendering primitives are internal implementation details; consumers configure a control such as `ZoorceNumericInput`, `ZoorceTimeline`, or `ZoorceDataGrid` directly and never select behavior through a page-wide switch statement.

The root `Zoorce.Web.UI` package is a convenience aggregate. Functional packages stay independently referenceable under `Zoorce.Web.UI.*`, so an application may use the layout without diagrams, timelines without the data grid, or form controls without the shell.

The sample is a normal Razor Pages host. It supplies navigation, endpoints, template examples, and realistic interaction flows; it does not reproduce or patch library markup, styling, or client behavior.

## Independent data lifecycle

`ZoorceDataController<T>` and the browser `ZoorceData` runtime provide a small transport-neutral boundary:

1. A component declares `ZoorceDataBindingOptions` with a source key/URI and an optional invalidation scope.
2. The browser runtime asks a registered source adapter for that component's data only.
3. The result updates the owning root; unrelated components preserve their DOM and state.
4. Any transport publishes a `ZoorceDataInvalidation` to the matching scope.

SignalR is one adapter, exposed by `Zoorce.Web.UI.Realtime.SignalR`. It translates a hub event into an invalidation envelope. Components themselves neither import the SignalR client nor contain a hub URL/event contract. Hosts can instead register a REST poller, GraphQL subscription, SSE bridge, cache notifier, or their own JavaScript source adapter.

The binding's `Loading` configuration is enabled by default. It selects initial skeleton/progress feedback, whether existing content remains readable during refresh, and a minimum display delay. Progress bars, circular progress, spinner/wait dialog, and skeleton components are supplied by Foundation and can also be used outside data-bound controls.

## Runtime themes

`ZoorceThemeService` owns a normalized active `ZoorceThemeDefinition`. The layout writes initial semantic CSS variables. `zoorce-theme.js` applies a later definition to the same variables so the entire family reacts consistently without static stylesheet recompilation. Component-level options change presentation or content; they do not fork the global palette.

## Quality gates

All public C# API projects generate XML documentation and treat missing public summaries as build failures. The `test:coverage` gate requires at least 90% total line coverage for authored C# across the current `Zoorce.Web.UI.*` family; Razor-generated code, test assemblies and the superseded pre-rename `Zoerce.WebUI` output are intentionally excluded. Unit tests cover normalization, lifecycle, data invalidation, optional adapters, and public component contracts. The sample receives browser checks at desktop and narrow breakpoints, covering loading, scoped refresh, interactive controls, keyboard paths, focus, and overflow.
