namespace Zoorce.Web.UI.Theming;

/// <summary>
/// Tracks and changes the active semantic theme for a Zoorce host.
/// </summary>
public interface IZoorceThemeService
{
    /// <summary>Gets the current normalized theme.</summary>
    ZoorceThemeDefinition Current { get; }

    /// <summary>Raised after the current theme changes.</summary>
    event EventHandler<ZoorceThemeDefinition>? ThemeChanged;

    /// <summary>Applies a new theme and notifies interested host infrastructure.</summary>
    void Apply(ZoorceThemeDefinition theme);
}
