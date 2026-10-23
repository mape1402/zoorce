namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Selects how a data-bound component applies an external data update.
/// </summary>
public enum ZoorceDataRefreshStrategy
{
    /// <summary>Reloads the component by querying its configured data source.</summary>
    Refresh,

    /// <summary>Replaces the local snapshot with an externally supplied snapshot.</summary>
    Replace,

    /// <summary>Applies an item-level mutation when the component can safely do so.</summary>
    Patch,

    /// <summary>Updates only when the host explicitly requests a refresh.</summary>
    Manual,

    /// <summary>Refreshes on the configured polling interval.</summary>
    Polling
}
