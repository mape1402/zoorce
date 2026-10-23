using System.Text.Json.Serialization;

namespace Zoorce.Web.UI.Diagrams;

/// <summary>Identifies the semantic rules and visual grammar used by a diagram.</summary>
public enum ZoorceDiagramProfile
{
    /// <summary>Represents the <c>Flowchart</c> option.</summary>
    Flowchart,
    /// <summary>Represents the <c>MindMap</c> option.</summary>
    MindMap,
    /// <summary>Represents the <c>OrgChart</c> option.</summary>
    OrgChart,
    /// <summary>Represents the <c>Architecture</c> option.</summary>
    Architecture
}

/// <summary>Controls whether a diagram can be explored, edited, or shown without interactive affordances.</summary>
public enum ZoorceDiagramMode
{
    /// <summary>Represents the <c>Viewer</c> option.</summary>
    Viewer,
    /// <summary>Represents the <c>ReadOnly</c> option.</summary>
    ReadOnly,
    /// <summary>Represents the <c>Editor</c> option.</summary>
    Editor
}

/// <summary>Controls the preferred starting arrangement for diagram nodes.</summary>
public enum ZoorceDiagramLayout
{
    /// <summary>Represents the <c>Manual</c> option.</summary>
    Manual,
    /// <summary>Represents the <c>Flow</c> option.</summary>
    Flow,
    /// <summary>Represents the <c>Radial</c> option.</summary>
    Radial,
    /// <summary>Represents the <c>TreeVertical</c> option.</summary>
    TreeVertical,
    /// <summary>Represents the <c>TreeHorizontal</c> option.</summary>
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
    /// <summary>Represents the <c>Neutral</c> option.</summary>
    Neutral,
    /// <summary>Represents the <c>Primary</c> option.</summary>
    Primary,
    /// <summary>Represents the <c>Success</c> option.</summary>
    Success,
    /// <summary>Represents the <c>Warning</c> option.</summary>
    Warning,
    /// <summary>Represents the <c>Danger</c> option.</summary>
    Danger,
    /// <summary>Represents the <c>Info</c> option.</summary>
    Info
}

/// <summary>Describes a declarative host action associated with a diagram element.</summary>
public enum ZoorceDiagramActionKind
{
    /// <summary>Represents the <c>None</c> option.</summary>
    None,
    /// <summary>Represents the <c>Navigate</c> option.</summary>
    Navigate,
    /// <summary>Represents the <c>Panel</c> option.</summary>
    Panel,
    /// <summary>Represents the <c>Modal</c> option.</summary>
    Modal,
    /// <summary>Represents the <c>Event</c> option.</summary>
    Event,
    /// <summary>Represents the <c>Command</c> option.</summary>
    Command,
    /// <summary>Represents the <c>Endpoint</c> option.</summary>
    Endpoint
}

/// <summary>Identifies a portable representation that can be produced from a diagram document.</summary>
public enum ZoorceDiagramExportFormat
{
    /// <summary>Represents the <c>Json</c> option.</summary>
    Json,
    /// <summary>Represents the <c>Svg</c> option.</summary>
    Svg,
    /// <summary>Represents the <c>Jpeg</c> option.</summary>
    Jpeg,
    /// <summary>Represents the <c>Html</c> option.</summary>
    Html,
    /// <summary>Represents the <c>Pdf</c> option.</summary>
    Pdf
}

/// <summary>Controls the background rendered behind an exported diagram.</summary>
public enum ZoorceDiagramExportBackground
{
    /// <summary>Represents the <c>Theme</c> option.</summary>
    Theme,
    /// <summary>Represents the <c>Transparent</c> option.</summary>
    Transparent,
    /// <summary>Represents the <c>Solid</c> option.</summary>
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
    /// <summary>Gets the configured <c>Start</c> value.</summary>
    public const string Start = "start";
    /// <summary>Gets the configured <c>End</c> value.</summary>
    public const string End = "end";
    /// <summary>Gets the configured <c>Process</c> value.</summary>
    public const string Process = "process";
    /// <summary>Gets the configured <c>Decision</c> value.</summary>
    public const string Decision = "decision";
    /// <summary>Gets the configured <c>InputOutput</c> value.</summary>
    public const string InputOutput = "input-output";
    /// <summary>Gets the configured <c>Subprocess</c> value.</summary>
    public const string Subprocess = "subprocess";
    /// <summary>Gets the configured <c>Document</c> value.</summary>
    public const string Document = "document";
    /// <summary>Gets the configured <c>DataStore</c> value.</summary>
    public const string DataStore = "data-store";
    /// <summary>Gets the configured <c>Delay</c> value.</summary>
    public const string Delay = "delay";
    /// <summary>Gets the configured <c>Connector</c> value.</summary>
    public const string Connector = "connector";
    /// <summary>Gets the configured <c>Annotation</c> value.</summary>
    public const string Annotation = "annotation";
    /// <summary>Gets the configured <c>Root</c> value.</summary>
    public const string Root = "root";
    /// <summary>Gets the configured <c>Branch</c> value.</summary>
    public const string Branch = "branch";
    /// <summary>Gets the configured <c>Topic</c> value.</summary>
    public const string Topic = "topic";
    /// <summary>Gets the configured <c>Note</c> value.</summary>
    public const string Note = "note";
    /// <summary>Gets the configured <c>Person</c> value.</summary>
    public const string Person = "person";
    /// <summary>Gets the configured <c>Team</c> value.</summary>
    public const string Team = "team";
    /// <summary>Gets the configured <c>Vacancy</c> value.</summary>
    public const string Vacancy = "vacancy";
    /// <summary>Gets the configured <c>User</c> value.</summary>
    public const string User = "user";
    /// <summary>Gets the configured <c>Service</c> value.</summary>
    public const string Service = "service";
    /// <summary>Gets the configured <c>Database</c> value.</summary>
    public const string Database = "database";
    /// <summary>Gets the configured <c>Queue</c> value.</summary>
    public const string Queue = "queue";
    /// <summary>Gets the configured <c>External</c> value.</summary>
    public const string External = "external";
    /// <summary>Gets the configured <c>Boundary</c> value.</summary>
    public const string Boundary = "boundary";
}
