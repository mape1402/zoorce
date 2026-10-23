namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Selects the visual treatment rendered while a component waits for data.
/// </summary>
public enum ZoorceLoadVisual
{
    /// <summary>Lets the component select its semantic default.</summary>
    Auto,

    /// <summary>Renders a layout-preserving skeleton.</summary>
    Skeleton,

    /// <summary>Renders a non-determinate spinner.</summary>
    Spinner,

    /// <summary>Renders a horizontal or circular progress indicator.</summary>
    Progress,

    /// <summary>Renders no loading chrome.</summary>
    None
}
