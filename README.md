# Zoorce

Reusable web shell and UI components for ASP.NET Core Razor Pages.

## Repository layout

- `src/Zoerce.WebUI.Abstractions` contains contracts shared by hosts and UI packages.
- `src/Zoerce.WebUI` is the Razor Class Library distributed as the `Zoerce.WebUI` package.
- `samples/Zoerce.WebUI.Sample` is a Razor Pages host used to exercise the package locally.
- `tests/Zoerce.WebUI.Tests` contains automated tests.
- `docs` holds architecture and usage documentation.

The first implementation milestone is a KnOwl-inspired application shell: configurable sidebar, topbar, theme, notifications, cards, and data grids.
