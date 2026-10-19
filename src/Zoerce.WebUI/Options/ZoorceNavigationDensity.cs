namespace Zoerce.WebUI.Options;

/// <summary>
/// Controls the vertical density of navigation entries in the application shell.
/// </summary>
public enum ZoorceNavigationDensity
{
    /// <summary>
    /// Uses generous spacing for short, presentation-focused navigation.
    /// </summary>
    Comfortable,

    /// <summary>
    /// Uses balanced spacing suitable for most business applications.
    /// </summary>
    Compact,

    /// <summary>
    /// Uses the smallest supported spacing for large operational menus.
    /// </summary>
    Dense
}
