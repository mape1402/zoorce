namespace Zoorce.Web.UI.Abstractions.Navigation;

/// <summary>
/// Controls the initial expansion of a navigation group.
/// </summary>
public enum NavigationGroupExpansion
{
    /// <summary>
    /// Uses the shell-level navigation expansion policy.
    /// </summary>
    Inherit,

    /// <summary>
    /// Renders the group expanded when the visitor has no saved preference.
    /// </summary>
    Expanded,

    /// <summary>
    /// Renders the group collapsed when the visitor has no saved preference.
    /// </summary>
    Collapsed
}
