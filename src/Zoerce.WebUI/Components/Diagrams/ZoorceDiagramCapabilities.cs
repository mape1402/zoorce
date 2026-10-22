namespace Zoerce.WebUI.Components.Diagrams;

/// <summary>
/// Defines the independently configurable actions exposed by a diagram surface.
/// A null capability inherits the safe default for the selected <see cref="ZoorceDiagramMode"/>.
/// </summary>
public sealed record ZoorceDiagramCapabilities
{
    /// <summary>Gets whether the palette, palette drag-and-drop, and node creation are available.</summary>
    public bool? CanAddNodes { get; init; }

    /// <summary>Gets whether nodes can be moved and layouts can be applied.</summary>
    public bool? CanMoveNodes { get; init; }

    /// <summary>Gets whether connections can be created or reconnected through node ports.</summary>
    public bool? CanEditConnections { get; init; }

    /// <summary>Gets whether node and connection semantic content can be edited.</summary>
    public bool? CanEditContent { get; init; }

    /// <summary>Gets whether node and connection presentation can be edited.</summary>
    public bool? CanEditAppearance { get; init; }

    /// <summary>Gets whether selected nodes and connections can be removed.</summary>
    public bool? CanRemoveElements { get; init; }

    /// <summary>Gets whether the diagram title and description can be edited.</summary>
    public bool? CanEditMetadata { get; init; }

    /// <summary>Gets whether the configured export experience is available.</summary>
    public bool? CanExport { get; init; }

    /// <summary>Gets whether a portable JSON document can replace the current diagram.</summary>
    public bool? CanImport { get; init; }

    /// <summary>Gets whether configured node actions may execute when a node is selected.</summary>
    public bool? CanInvokeNodeActions { get; init; }

    /// <summary>Gets whether any configured capability can modify the diagram document.</summary>
    public bool CanModifyDocument => CanAddNodes == true
        || CanMoveNodes == true
        || CanEditConnections == true
        || CanEditContent == true
        || CanEditAppearance == true
        || CanRemoveElements == true
        || CanEditMetadata == true
        || CanImport == true;

    /// <summary>Gets whether a node or edge inspector can expose editable fields.</summary>
    public bool CanUseInspector => CanEditContent == true || CanEditAppearance == true;

    /// <summary>Resolves inherited capability values without changing explicit host configuration.</summary>
    internal ZoorceDiagramCapabilities Resolve(ZoorceDiagramMode mode, bool exportEnabled)
    {
        var editor = mode == ZoorceDiagramMode.Editor;
        return this with
        {
            CanAddNodes = CanAddNodes ?? editor,
            CanMoveNodes = CanMoveNodes ?? editor,
            CanEditConnections = CanEditConnections ?? editor,
            CanEditContent = CanEditContent ?? editor,
            CanEditAppearance = CanEditAppearance ?? editor,
            CanRemoveElements = CanRemoveElements ?? editor,
            CanEditMetadata = CanEditMetadata ?? editor,
            CanExport = CanExport ?? exportEnabled,
            CanImport = CanImport ?? false,
            CanInvokeNodeActions = CanInvokeNodeActions ?? true
        };
    }
}
