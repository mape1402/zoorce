namespace Zoerce.WebUI.Abstractions.Navigation;

/// <summary>
/// Defines how a navigation item determines its current state.
/// </summary>
public enum NavigationMatchMode
{
    /// <summary>
    /// Uses an exact match for the root route, a prefix match for regular routes,
    /// and client-side hash matching when a target contains a fragment.
    /// </summary>
    Auto,

    /// <summary>
    /// Matches the request path exactly.
    /// </summary>
    Exact,

    /// <summary>
    /// Matches the request path and its descendants.
    /// </summary>
    Prefix,

    /// <summary>
    /// Matches the browser fragment on the client.
    /// </summary>
    Hash,

    /// <summary>
    /// Never applies a current state automatically.
    /// </summary>
    None
}
