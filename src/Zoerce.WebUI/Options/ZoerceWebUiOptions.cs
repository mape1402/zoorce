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
    /// Gets the light palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePaletteOptions Light { get; } = ZoorceThemePaletteOptions.CreateLight();

    /// <summary>
    /// Gets the dark palette exposed as semantic CSS variables.
    /// </summary>
    public ZoorceThemePaletteOptions Dark { get; } = ZoorceThemePaletteOptions.CreateDark();

    /// <summary>
    /// Gets CSS rules that make both configured palettes available to the web shell.
    /// </summary>
    public string ToCssThemeRules()
        => $":root[data-zoerce-theme=\"light\"] {{{Light.ToCssVariables()}}}" +
           $":root[data-zoerce-theme=\"dark\"] {{{Dark.ToCssVariables()}}}";
}
