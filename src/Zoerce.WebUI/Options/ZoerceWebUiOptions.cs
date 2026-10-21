using Zoerce.WebUI.Components.Diagrams;

namespace Zoerce.WebUI.Options;

/// <summary>
/// Configures shared behavior for the Zoorce web UI package.
/// </summary>
public sealed class ZoerceWebUiOptions
{
    /// <summary>
    /// Gets or sets the product name shown by the future web shell.
    /// </summary>
    public string ApplicationName { get; set; } = "Zoorce";

    /// <summary>
    /// Gets or sets the color mode used when the visitor has no saved preference.
    /// </summary>
    public ZoorceThemeMode DefaultTheme { get; set; } = ZoorceThemeMode.Dark;

    /// <summary>
    /// Gets or sets the brand mark rendered at the top of the sidebar.
    /// </summary>
    public string BrandMark { get; set; } = "Z";

    /// <summary>
    /// Gets or sets the sidebar behavior for desktop-sized viewports.
    /// </summary>
    public ZoorceSidebarMode SidebarMode { get; set; } = ZoorceSidebarMode.Collapsible;

    /// <summary>
    /// Gets or sets whether a collapsible desktop sidebar starts in its compact state
    /// when the visitor has no previously saved preference.
    /// </summary>
    public bool SidebarCollapsedByDefault { get; set; }

    /// <summary>
    /// Gets or sets whether the visitor's desktop sidebar preference is kept locally.
    /// </summary>
    public bool PersistSidebarState { get; set; } = true;

    /// <summary>
    /// Gets or sets the vertical density used by the sidebar navigation.
    /// </summary>
    public ZoorceNavigationDensity NavigationDensity { get; set; } = ZoorceNavigationDensity.Compact;

    /// <summary>
    /// Gets or sets the default export policy used by diagrams that do not provide a local export configuration.
    /// </summary>
    public ZoorceDiagramExportOptions DiagramExport { get; set; } = new();

    /// <summary>
    /// Gets the light palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePaletteOptions Light { get; } = ZoorceThemePaletteOptions.CreateLight();

    /// <summary>
    /// Gets the dark palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePaletteOptions Dark { get; } = ZoorceThemePaletteOptions.CreateDark();

    /// <summary>Applies a complete semantic palette pair to this UI configuration.</summary>
    public ZoerceWebUiOptions UseThemePreset(ZoorceThemePreset preset)
    {
        ZoorceThemePresets.Apply(this, preset);
        return this;
    }

    /// <summary>
    /// Gets CSS rules that make both configured palettes available to the web shell.
    /// </summary>
    public string ToCssThemeRules()
        => $":root[data-zoerce-theme=\"light\"] {{{Light.ToCssVariables()}}}" +
           $":root[data-zoerce-theme=\"dark\"] {{{Dark.ToCssVariables()}}}";
}
