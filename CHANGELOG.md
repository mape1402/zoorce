# Changelog

All notable changes to ZoorceCode are documented in this file.

## [v1.1.0] - 2026-10-25

### Added

- An optional top-bar back action configured globally or per page, with a host-provided destination or behavior.
- Valky, an animated help companion with customizable content and idle, thinking, talking and celebrating states.
- A configurable standalone sign-in page that delegates authentication to the host application.
- A reusable standalone status page with configurable content, actions and an animated astronaut, including defaults for 401, 403, 404, 500 and 503.

### Changed

- Reduced the top bar height to make more vertical space available for application content.

### Fixed

- The infinite-scroll data grid header now extends over the reserved scrollbar gutter before and after overflow appears.
- Data grid rows retain their bottom separator when there is only one row or when the row is the last one displayed.

## [v1.0.0] - 2026-10-24

### Added

- The `Zoorce.Web.UI` aggregate package and independently consumable packages for abstractions, data, theming, foundation, layout, navigation, forms, timelines, charts, data grids, diagrams and optional SignalR invalidation.
- Typed, encapsulated Razor components with scoped client behavior, keyboard interaction and host-owned configuration contracts.
- Complete light and dark semantic palettes, preset themes and runtime token switching.
- Date/time selection, drawers, notifications, breadcrumbs, pagination, split panes, workflow steppers and wizards.
- A sample application that demonstrates and functionally exercises the component catalog.
- Automated build, CSS generation, test coverage enforcement, package creation and marker-driven GitHub and NuGet releases.
