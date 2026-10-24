namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Defines the built-in visual treatment for a dialog header.</summary>
public enum ZoorceDialogHeaderMode
{
    /// <summary>Shows the eyebrow, title, description, and close control.</summary>
    Standard,

    /// <summary>Shows only the title and close control.</summary>
    Compact,

    /// <summary>Omits the visual header while retaining the title for assistive technology.</summary>
    None
}
