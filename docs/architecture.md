# Architecture

Zoorce is organized as a reusable Razor Class Library rather than a copied layout per application.

`Zoerce.WebUI.Abstractions` owns the host-facing contracts. `Zoerce.WebUI` owns Razor markup, static web assets, styling, and service-registration extensions. Applications consume the package, provide navigation and user context, and retain ownership of their pages and business domain.

This separation keeps the web shell independent from a specific authentication provider, database, or business module.
