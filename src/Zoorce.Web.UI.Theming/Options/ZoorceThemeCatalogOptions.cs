namespace Zoorce.Web.UI.Theming;

/// <summary>
/// Configures the semantic token sets available to a Zoorce host.
/// </summary>
public sealed class ZoorceThemeCatalogOptions
{
    /// <summary>Gets the token palette used by the light color mode.</summary>
    public ZoorceThemePalette Light { get; } = ZoorceThemePalette.CreateLight();

    /// <summary>Gets the token palette used by the dark color mode.</summary>
    public ZoorceThemePalette Dark { get; } = ZoorceThemePalette.CreateDark();

    /// <summary>Applies one of Zoorce's complete, accessible palette pairs.</summary>
    public ZoorceThemeCatalogOptions UsePreset(ZoorceThemePreset preset)
    {
        ZoorceThemePresets.Apply(this, preset);
        return this;
    }

    /// <summary>Creates a named runtime definition for the selected color mode.</summary>
    public ZoorceThemeDefinition CreateDefinition(ZoorceThemeMode mode)
        => new()
        {
            Name = mode.ToString().ToLowerInvariant(),
            Tokens = (mode == ZoorceThemeMode.Light ? Light : Dark).ToTokenDictionary()
        };

    /// <summary>Returns the CSS rules that make both configured color modes available without a page reload.</summary>
    public string ToCssRules()
        => $":root[data-zoorce-theme=\"light\"] {{{Light.ToCssVariables()}}}" +
           $":root[data-zoorce-theme=\"dark\"] {{{Dark.ToCssVariables()}}}";
}
