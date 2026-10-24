using Zoorce.Web.UI.Theming;
using Zoorce.Web.UI.Foundation.Components;

namespace Zoorce.Web.UI.Layout;

/// <summary>
/// Configures the reusable Zoorce application shell.
/// </summary>
public sealed class ZoorceShellOptions
{
    /// <summary>
    /// Gets or sets the product name shown by the application shell.
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
    /// Gets or sets the optional notification center rendered in the application top bar.
    /// </summary>
    public ZoorceNotificationCenterOptions? NotificationCenter { get; set; }

    /// <summary>Gets the central palette catalog consumed by every Zoorce package.</summary>
    public ZoorceThemeCatalogOptions Themes { get; } = new();

    /// <summary>
    /// Gets the light palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePalette Light => Themes.Light;

    /// <summary>
    /// Gets the dark palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePalette Dark => Themes.Dark;

    /// <summary>Applies a complete semantic palette pair to the central theme catalog.</summary>
    public ZoorceShellOptions UseThemePreset(ZoorceThemePreset preset)
    {
        Themes.UsePreset(preset);
        return this;
    }

    /// <summary>
    /// Gets CSS rules that make both configured palettes available to the web shell.
    /// </summary>
    public string ToCssThemeRules() => Themes.ToCssRules();
}
