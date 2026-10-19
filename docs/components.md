# Component catalog

Zoorce provides a dark-first, semantic-token UI foundation for Razor Pages. The sample project demonstrates each pattern in a single running application.

## Shell and navigation

- Application shell, responsive sidebar, topbar, product brand and user profile surface.
- Desktop sidebar that can hide completely, preserves the visitor preference, and can be configured as fixed instead.
- Grouped navigation, current-page states, mobile overlay, skip link and quick command palette.
- Light/Dark switcher, with colors injected through `ZoerceWebUiOptions` in dependency injection.

Navigation entries come from the host application's `INavigationProvider`; Zoorce never hard-codes product sections. Use `NavigationMatchMode.Exact` for a single route, `Prefix` for a route branch, `Hash` for a one-page section, `None` for a passive item, or `Auto` for the default behavior. `ActiveUrls` can add aliases without changing the visible target.

## Actions and feedback

- Primary, secondary, ghost, danger, icon and disabled button treatments.
- Client and server-backed toasts via `IToastService`.
- Confirmation dialog, modal, drawer, tooltip, skeleton loader and empty state.

## Data and content

- Metric cards, insight cards, progress indicators, charts, status chips and badges.
- Data grid with search, filter chips, row actions and pagination controls.
- Documentation layout with navigation, table of contents, callouts, code blocks and copy control.

## Forms

- Text, select, combobox-style datalist, date, date range and textarea inputs.
- Hints, validation/error presentation, checkboxes, radios and switches.
- Tabs and tab panels.

All visual colors consume `--z-*` semantic variables emitted by the Razor layout, including the foreground and semantic success/warning/danger/info states, so a host can brand both themes without editing Zoorce's generated Tailwind CSS.
