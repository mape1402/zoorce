namespace Zoerce.WebUI.Options;

/// <summary>
/// Defines how the sidebar behaves on desktop-sized viewports.
/// </summary>
public enum ZoorceSidebarMode
{
    /// <summary>
    /// Lets visitors switch between the expanded sidebar and a compact icon rail.
    /// </summary>
    Collapsible,

    /// <summary>
    /// Keeps the desktop sidebar expanded. Mobile navigation remains an overlay.
    /// </summary>
    Fixed
}
