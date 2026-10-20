namespace Zoerce.WebUI.Components.DataGrid;

/// <summary>Controls how a DataGrid reacts when its optional realtime transport publishes a change.</summary>
public enum ZoorceDataGridRealtimeBehavior
{
    /// <summary>Shows a non-disruptive update notice until the user elects to refresh the query.</summary>
    Badge,

    /// <summary>Applies changes only when the affected row is already loaded and visible.</summary>
    Patch,

    /// <summary>Reports that data has changed without mutating the current result set.</summary>
    Manual,

    /// <summary>Requeries the current result set. Use only when the host explicitly accepts that behavior.</summary>
    Refresh
}
