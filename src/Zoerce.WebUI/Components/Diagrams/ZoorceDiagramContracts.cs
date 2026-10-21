using System.Text.Json.Serialization;

namespace Zoerce.WebUI.Components.Diagrams;

/// <summary>Identifies the semantic rules and visual grammar used by a diagram.</summary>
public enum ZoorceDiagramProfile
{
    Flowchart,
    MindMap,
    OrgChart,
    Architecture
}

/// <summary>Controls whether a diagram can be explored, edited, or shown without interactive affordances.</summary>
public enum ZoorceDiagramMode
{
    Viewer,
    ReadOnly,
    Editor
}

/// <summary>Controls the preferred starting arrangement for diagram nodes.</summary>
public enum ZoorceDiagramLayout
{
    Manual,
    Flow,
    Radial,
    TreeVertical,
    TreeHorizontal
}

/// <summary>Controls how a pointer gesture over unused canvas space moves the diagram viewport.</summary>
public enum ZoorceDiagramPanMode
{
    /// <summary>Dragging unused canvas space moves the viewport.</summary>
    Background,
    /// <summary>Viewport movement requires holding the space key or using the middle mouse button.</summary>
    SpaceKey,
    /// <summary>Viewport movement requires the middle mouse button.</summary>
    MiddleButton,
    /// <summary>Disables pointer panning while preserving keyboard navigation and zoom controls.</summary>
    Disabled
}

/// <summary>Defines a connection anchor on one of the four sides of a diagram node.</summary>
public enum ZoorceDiagramPortSide
{
    /// <summary>Lets the diagram choose the most appropriate anchor for the profile and relative position.</summary>
    Auto,
    /// <summary>Uses the top-center anchor.</summary>
    Top,
    /// <summary>Uses the right-center anchor.</summary>
    Right,
    /// <summary>Uses the bottom-center anchor.</summary>
    Bottom,
    /// <summary>Uses the left-center anchor.</summary>
    Left
}

/// <summary>Provides semantic visual tones for nodes and connections.</summary>
public enum ZoorceDiagramTone
{
    Neutral,
    Primary,
    Success,
    Warning,
    Danger,
    Info
}

/// <summary>Describes a declarative host action associated with a diagram element.</summary>
public enum ZoorceDiagramActionKind
{
    None,
    Navigate,
    Panel,
    Modal,
    Event,
    Command,
    Endpoint
}

/// <summary>Identifies a portable representation that can be produced from a diagram document.</summary>
public enum ZoorceDiagramExportFormat
{
    Json,
    Svg,
    Jpeg,
    Html,
    Pdf
}

/// <summary>Controls the background rendered behind an exported diagram.</summary>
public enum ZoorceDiagramExportBackground
{
    Theme,
    Transparent,
    Solid
}

/// <summary>Controls how PDF output is produced for a diagram export.</summary>
public enum ZoorceDiagramPdfMode
{
    /// <summary>Opens a print-ready vector view so the browser can save it as a PDF.</summary>
    BrowserPrint,
    /// <summary>Posts the export payload to a host-provided endpoint that returns a PDF file.</summary>
    Server
}

/// <summary>Defines a point in the diagram's vector coordinate system.</summary>
public sealed record ZoorceDiagramPoint(decimal X, decimal Y);

/// <summary>Defines a host-controlled, non-script action for a diagram element.</summary>
public sealed record ZoorceDiagramAction(
    ZoorceDiagramActionKind Kind,
    string? Target = null,
    IReadOnlyDictionary<string, string?>? Payload = null);

/// <summary>Defines a two-stop color gradient that can be applied to a diagram node.</summary>
public sealed record ZoorceDiagramGradient(
    string StartColor,
    string EndColor,
    int Angle = 135);

/// <summary>Defines optional presentation overrides for an individual diagram node.</summary>
public sealed record ZoorceDiagramNodeStyle(
    string? FillColor = null,
    string? BorderColor = null,
    decimal? BorderWidth = null,
    string? TextColor = null,
    string? FontFamily = null,
    decimal? FontSize = null,
    int? FontWeight = null,
    decimal? CornerRadius = null,
    decimal? Opacity = null,
    ZoorceDiagramGradient? Gradient = null);

/// <summary>Defines optional presentation overrides for an individual connection.</summary>
public sealed record ZoorceDiagramEdgeStyle(
    string? StrokeColor = null,
    decimal? StrokeWidth = null,
    bool Dashed = false);

/// <summary>Defines one semantic node in a serializable diagram document.</summary>
public sealed record ZoorceDiagramNode(
    string Id,
    string Kind,
    string Title,
    ZoorceDiagramPoint Position,
    string? Description = null,
    ZoorceDiagramTone Tone = ZoorceDiagramTone.Neutral,
    bool Collapsed = false,
    ZoorceDiagramAction? Action = null,
    IReadOnlyDictionary<string, string?>? Data = null,
    ZoorceDiagramNodeStyle? Style = null,
    string? Identifier = null);

/// <summary>Defines a directed connection between two diagram nodes.</summary>
public sealed record ZoorceDiagramEdge(
    string Id,
    string FromNodeId,
    string ToNodeId,
    string? Label = null,
    ZoorceDiagramTone Tone = ZoorceDiagramTone.Neutral,
    string? Kind = null,
    ZoorceDiagramEdgeStyle? Style = null,
    ZoorceDiagramPortSide FromPort = ZoorceDiagramPortSide.Auto,
    ZoorceDiagramPortSide ToPort = ZoorceDiagramPortSide.Auto);

/// <summary>Defines a serializable, versioned diagram document.</summary>
public sealed record ZoorceDiagramDocument(
    int SchemaVersion = 1,
    IReadOnlyList<ZoorceDiagramNode>? Nodes = null,
    IReadOnlyList<ZoorceDiagramEdge>? Edges = null,
    string? Version = null,
    string? Title = null,
    string? Description = null)
{
    /// <summary>Gets nodes as a never-null collection for hosts that build documents incrementally.</summary>
    [JsonIgnore]
    public IReadOnlyList<ZoorceDiagramNode> NodeList => Nodes ?? [];

    /// <summary>Gets connections as a never-null collection for hosts that build documents incrementally.</summary>
    [JsonIgnore]
    public IReadOnlyList<ZoorceDiagramEdge> EdgeList => Edges ?? [];
}

/// <summary>Provides built-in node-kind names shared by the supported diagram profiles.</summary>
public static class ZoorceDiagramNodeKinds
{
    public const string Start = "start";
    public const string End = "end";
    public const string Process = "process";
    public const string Decision = "decision";
    public const string InputOutput = "input-output";
    public const string Subprocess = "subprocess";
    public const string Document = "document";
    public const string DataStore = "data-store";
    public const string Delay = "delay";
    public const string Connector = "connector";
    public const string Annotation = "annotation";
    public const string Root = "root";
    public const string Branch = "branch";
    public const string Topic = "topic";
    public const string Note = "note";
    public const string Person = "person";
    public const string Team = "team";
    public const string Vacancy = "vacancy";
    public const string User = "user";
    public const string Service = "service";
    public const string Database = "database";
    public const string Queue = "queue";
    public const string External = "external";
    public const string Boundary = "boundary";
}
