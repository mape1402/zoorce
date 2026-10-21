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

/// <summary>Defines a point in the diagram's vector coordinate system.</summary>
public sealed record ZoorceDiagramPoint(decimal X, decimal Y);

/// <summary>Defines a host-controlled, non-script action for a diagram element.</summary>
public sealed record ZoorceDiagramAction(
    ZoorceDiagramActionKind Kind,
    string? Target = null,
    IReadOnlyDictionary<string, string?>? Payload = null);

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
    IReadOnlyDictionary<string, string?>? Data = null);

/// <summary>Defines a directed connection between two diagram nodes.</summary>
public sealed record ZoorceDiagramEdge(
    string Id,
    string FromNodeId,
    string ToNodeId,
    string? Label = null,
    ZoorceDiagramTone Tone = ZoorceDiagramTone.Neutral,
    string? Kind = null);

/// <summary>Defines a serializable, versioned diagram document.</summary>
public sealed record ZoorceDiagramDocument(
    int SchemaVersion = 1,
    IReadOnlyList<ZoorceDiagramNode>? Nodes = null,
    IReadOnlyList<ZoorceDiagramEdge>? Edges = null,
    string? Version = null)
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
