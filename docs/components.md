# Component catalog

## Foundation and layout

`Zoorce.Web.UI.Foundation` contains reusable progress bars (linear and circular), skeletons, wait dialog/spinner, confirmation dialog, generic dialog, toast region, buttons, progress buttons, floating actions, speed dial, dropdown menus, avatars, badges, cards, message surfaces, and chips. `Zoorce.Web.UI.Layout` composes those primitives into a semantic application shell; sidebar navigation, top bar, user menu, command palette, and layout regions remain separately reusable components.

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

The current catalog includes text, textarea, password, hidden, masked, numeric up/down, quantity stepper, currency, percentage, date/time/range/duration, sliders, checkbox, toggle, single/multi-select, searchable multi-select dropdown, select, combobox, autocomplete, multi-column combobox, listbox, dropdown tree, mention editor, radio/checkbox/chip groups, segmented control, transfer list, rating, tags, color, icon, emoji, phone, address, OTP, key/value, file upload, image gallery, and single-image avatar picker.

Named options expose the common label/value/state contract plus only relevant settings. File/image controls support accepted formats, one or many files, size/count limits, deferred/manual/automatic same-origin upload, persisted assets, and progress events. Avatar is intentionally a single image: its visual target opens selection, Edit replaces the image, and Remove clears it.

Form enhancement is progressive. Every field keeps an ordinary named HTML input and emits bubbling `zoorce:form:*` events only within its containing form/control scope.

`ZoorceSelect` owns its trigger, chevron, listbox and submitted hidden value, so its appearance and interaction do not vary by browser. Set `AllowClear` for an optional clear action. `ZoorceCombobox` owns filtering, its disclosure button, keyboard navigation and empty-result state; `AllowCustomValues`, `OpenOnFocus`, and `EmptyResultsText` configure its behavior. Both controls can render richer option content through `ChoiceTemplate` while retaining their selection semantics.

`ZoorceRating` stores one selected value but uses cumulative visual fill by default: a value of four fills icons one through four. `DisplayMode`, `AllowClear`, choice labels, and choice icons make the control suitable for other scales. Arrow keys, Home, End, Enter, and Space work without requiring a pointer.

## Navigation controls

`Zoorce.Web.UI.Navigation` also provides accordion, tab set, tree view, menu bar, context menu, and toolbar components. They use their own roots for roving focus, expansion and selection state, and emit bubbling `zoorce:navigation:*` events. Context menus attach to a host selector and position themselves only while open; all other controls preserve ordinary server-rendered content when JavaScript is unavailable.

## Wizard

`zoorce-wizard` is a content-bearing workflow control. It owns the progress rail, exactly one active Razor panel, field-error region, and persistent action footer. Use `ZoorceWizardLayout.Sidebar` for a left rail beside content or `ZoorceWizardLayout.Top` for a horizontal rail above content. Every configured `ZoorceWizardStep` requires one nested `zoorce-wizard-panel` with the matching `step-id`.

```cshtml
@using Zoorce.Web.UI.Navigation.Stepper
@using Zoorce.Web.UI.Navigation.Wizard
@{
    var wizard = new ZoorceWizardOptions
    {
        Id = "workspace-setup",
        Layout = ZoorceWizardLayout.Sidebar,
        ExecutionMode = ZoorceWizardExecutionMode.Manual,
        ShowSaveDraft = true,
        Steps =
        [
            new("identity", "Workspace identity", FormId: "identity-form", State: ZoorceStepperState.Active),
            new("members", "Invite members", FormId: "members-form"),
            new("review", "Review", FormId: "review-form", FinishLabel: "Create workspace")
        ]
    };
}

<zoorce-wizard options="wizard">
    <zoorce-wizard-panel step-id="identity"><form id="identity-form">...</form></zoorce-wizard-panel>
    <zoorce-wizard-panel step-id="members"><form id="members-form">...</form></zoorce-wizard-panel>
    <zoorce-wizard-panel step-id="review"><form id="review-form">...</form></zoorce-wizard-panel>
</zoorce-wizard>
```

In `Manual` mode the component emits `zoorce:wizard:request-change`, `zoorce:wizard:request-save`, `zoorce:wizard:request-cancel`, and `zoorce:wizard:request-finish`. The host owns validation and calls `ZoorceWizards.get(id).commit(stepId, action)` or `.complete(message)` only after approval. In `HostManaged` mode, set `TransitionUrl`; the browser posts `ZoorceWizardTransitionRequest` to that same-origin endpoint and applies the returned `ZoorceWizardTransitionDecision`. A rejected decision can return `FieldErrors` keyed by input name or id.

The wizard and stepper use the same navigation rules (`ZoorceStepperTransitionRules`) and semantic step states/tones. This keeps forward/backward, warning, error, and authorization behavior aligned while the wizard provides the form panels and footer actions that a standalone stepper intentionally does not own.

Backward transitions never run form validation. Activating a step in either rail is normalized to the same semantic `next` or `previous` transition as the footer, so a forward click cannot bypass host validation. Validate forward transitions and finishing in the host, then call `commit`; this lets someone safely return to correct an earlier panel.

For a contained workflow, independently set `PanelHeaderMode` (`Full`, `Title`, or `Hidden`), `ProgressSpacing`, `ContentSpacing`, and `FooterSpacing` (`Standard`, `Compact`, or `None`), and `ContentHeight` (`Fill` or `Auto`). `Hidden` retains the active panel name for assistive technology and sends focus to the first enabled control instead of rendering duplicate step copy. `Auto` prevents a short dialog form from reserving the full-page workspace height.

`ZoorceDialog` is composable rather than prescriptive: `HeaderMode` supports `Standard`, `Compact`, and `None`; `Header` replaces the generated visual header; `ShowCloseButton` controls the dismiss action; and `BodyPadding` supports `Default`, `Compact`, and `None`. Keep a `Title` even with `HeaderMode.None`, because it remains the dialog's accessible name. A dense wizard dialog normally uses a headerless, padding-free Dialog together with a wizard using hidden panel headers and compact spacing.

Native `input`, `select`, and `textarea` elements placed directly inside a `z-form-field` in a wizard panel receive the wizard's token-driven control surface, including hover, focus, validation, and light/dark-theme contrast. Existing Zoorce form components retain their own styles.
