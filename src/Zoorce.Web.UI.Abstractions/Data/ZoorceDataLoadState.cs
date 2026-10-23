namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Describes the lifecycle state of an isolated data-bound component instance.
/// </summary>
public enum ZoorceDataLoadState
{
    /// <summary>The component has not requested data yet.</summary>
    Idle,

    /// <summary>The component is waiting for its first result.</summary>
    Loading,

    /// <summary>The component is refreshing while preserving a previous result.</summary>
    Refreshing,

    /// <summary>The component has a non-empty result.</summary>
    Success,

    /// <summary>The component has completed a request without any items.</summary>
    Empty,

    /// <summary>The component could not complete its latest request.</summary>
    Error,

    /// <summary>The component has released its resources and cannot load again.</summary>
    Disposed
}
