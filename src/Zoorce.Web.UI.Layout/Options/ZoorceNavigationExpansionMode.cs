namespace Zoorce.Web.UI.Layout;

/// <summary>
/// Defines the initial expansion policy for sidebar navigation groups.
/// </summary>
public enum ZoorceNavigationExpansionMode
{
    /// <summary>
    /// Opens only groups that contain the active route.
    /// </summary>
    ActivePath,

    /// <summary>
    /// Starts every group collapsed unless that group overrides the policy.
    /// </summary>
    AllCollapsed,

    /// <summary>
    /// Starts every group expanded unless that group overrides the policy.
    /// </summary>
    AllExpanded
}
