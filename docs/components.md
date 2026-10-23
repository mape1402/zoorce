# Component catalog

## Foundation and layout

`Zoorce.Web.UI.Foundation` contains reusable progress bars (linear and circular), skeletons, wait dialog/spinner, confirmation dialog, toast region, buttons, message surfaces, and chips. `Zoorce.Web.UI.Layout` composes those primitives into a semantic application shell; sidebar navigation, top bar, user menu, command palette, and layout regions remain separately reusable components.

The shell receives `ZoorceShellOptions` from the host. `ZoorceThemeCatalogOptions` supplies the initial palette while `IZoorceThemeService` supports a runtime replacement. Do not copy layout markup into a sample or host page—use `ZoorceLayout` and customize providers/options instead.

## Data-bound components

Grid, charts, timelines, steppers, and diagrams expose `Binding`. A binding is not tied to any backend technology:

```cshtml
@await Component.InvokeAsync("ZoorceDataGrid", new ZoorceDataGridOptions
{
    Id = "orders",
    AriaLabel = "Orders",
    Binding = new ZoorceDataBindingOptions
    {
        Source = "/api/orders",
        InvalidationScope = "orders"
    },
    LoadMode = ZoorceDataLoadMode.InfiniteScroll,
    Columns =
    [
        new("number", "Order"),
        new("status", "Status")
    ]
})
```

The browser data runtime registers source adapters and scoped subscribers. An HTTP adapter is included. A host can call `ZoorceData.register("orders", adapter)` for another source type, and can publish `{ scope, strategy, change }` through `ZoorceData.invalidate(...)`. SignalR is optional: its adapter bridges a hub event to that same function.

`ZoorceLoadingOptions` is enabled by default. Initial loads use skeleton/progress feedback and refreshes retain the last useful content unless an instance chooses otherwise. A grid, chart, timeline, or diagram updates its own root only.

### Templates

`ZoorceDataGridColumn` can declare a cell template key and `ZoorceTimelineItem` can choose an `ItemTemplates` key. The host provides the Razor partial/template once; each data item is rendered inside the component’s isolated root. This provides custom columns and timeline cards without reimplementing paging, keyboard behavior, overflow, data binding, or realtime invalidation.

## Timeline

`ZoorceTimeline` supports activity, agenda, rail, journal, milestones, horizontal, and presentation variants through `ZoorceTimelineVariant`, `Orientation`, and overflow configuration. Use `Interaction` to select read-only, selectable, actionable, or navigable behavior. The component dispatches scoped item events, so a host can open a detail panel or navigate without a global listener.

When a horizontal sequence exceeds its viewport, the instance creates its own horizontal scroll surface. It never converts a long timeline into a compressed set of overlapping cards; controls and focus stay local to that instance.

## Diagram workspace

`ZoorceDiagram` supports flowchart, mind map, organization chart, and architecture profiles. It renders a portable `ZoorceDiagramDocument` with typed nodes, ports, edges, appearance, permissions, and export options. Node/edge changes remain local to the component’s document and can be saved through an optional host endpoint.

Read-only behavior is capability-based: hosts independently allow adding nodes, moving nodes/connectors, editing content, editing appearance, importing, and exporting. Editor-only controls are not merely disabled; they are absent when their capability is unavailable. Tooltips, minimap, zoom, focus mode, connector anchors, and keyboard shortcuts stay encapsulated in the diagram root.

## Forms

Each form field has a dedicated View Component and typed options contract. Examples:

```cshtml
@await Component.InvokeAsync("ZoorceQuantityStepper", new ZoorceQuantityStepperOptions
{
    Id = "dependents",
    Name = "dependents",
    Label = "Number of dependents",
    Value = "3",
    Minimum = 0,
    Maximum = 12,
    Step = 1
})

@await Component.InvokeAsync("ZoorceAvatarPicker", new ZoorceAvatarPickerOptions
{
    Id = "owner-avatar",
    Name = "avatar",
    Accept = "image/png,image/jpeg,image/webp",
    MaximumFileSizeBytes = 2 * 1024 * 1024
})
```

The current catalog includes text, textarea, password, hidden, masked, numeric up/down, quantity stepper, currency, percentage, date/time/range/duration, sliders, checkbox, toggle, single/multi-select, combobox, radio/checkbox/chip groups, segmented control, transfer list, rating, tags, color, icon, emoji, phone, address, OTP, key/value, file upload, image gallery, and single-image avatar picker.

Named options expose the common label/value/state contract plus only relevant settings. File/image controls support accepted formats, one or many files, size/count limits, deferred/manual/automatic same-origin upload, persisted assets, and progress events. Avatar is intentionally a single image: its visual target opens selection, Edit replaces the image, and Remove clears it.

Form enhancement is progressive. Every field keeps an ordinary named HTML input and emits bubbling `zoorce:form:*` events only within its containing form/control scope.

`ZoorceSelect` owns its trigger, chevron, listbox and submitted hidden value, so its appearance and interaction do not vary by browser. Set `AllowClear` for an optional clear action. `ZoorceCombobox` owns filtering, its disclosure button, keyboard navigation and empty-result state; `AllowCustomValues`, `OpenOnFocus`, and `EmptyResultsText` configure its behavior. Both controls can render richer option content through `ChoiceTemplate` while retaining their selection semantics.

`ZoorceRating` stores one selected value but uses cumulative visual fill by default: a value of four fills icons one through four. `DisplayMode`, `AllowClear`, choice labels, and choice icons make the control suitable for other scales. Arrow keys, Home, End, Enter, and Space work without requiring a pointer.
