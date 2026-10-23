using Zoorce.Web.UI.Abstractions.Data;

namespace Zoorce.Web.UI.Diagrams;

/// <summary>Configures one isolated, progressively enhanced diagram surface.</summary>
public sealed record ZoorceDiagramOptions
{
    /// <summary>Gets the unique DOM identifier for this diagram instance.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the visible title shown above the diagram.</summary>
    public string Title { get; init; } = "Diagram";

    /// <summary>Gets the accessible summary announced for the diagram surface.</summary>
    public string Summary { get; init; } = "Interactive diagram";

    /// <summary>Gets the semantic profile that defines the diagram's grammar.</summary>
    public ZoorceDiagramProfile Profile { get; init; } = ZoorceDiagramProfile.Flowchart;

    /// <summary>Gets the interaction level offered by the instance.</summary>
    public ZoorceDiagramMode Mode { get; init; } = ZoorceDiagramMode.Editor;

    /// <summary>Gets the preferred initial node layout.</summary>
    public ZoorceDiagramLayout Layout { get; init; } = ZoorceDiagramLayout.Manual;

    /// <summary>Gets the initial server-rendered diagram document.</summary>
    public ZoorceDiagramDocument Document { get; init; } = new();

    /// <summary>Gets the transport-agnostic binding that loads and invalidates this document.</summary>
    public ZoorceDataBindingOptions Binding { get; init; } = new();

    /// <summary>Gets an optional REST endpoint that accepts a full document save.</summary>
    public string? SaveUrl { get; init; }

    /// <summary>Gets an optional REST endpoint used for host-specific validation.</summary>
    public string? ValidateUrl { get; init; }

    /// <summary>Gets whether the editor palette is visible when editing is enabled.</summary>
    public bool ShowPalette { get; init; } = true;

    /// <summary>Gets whether the selection inspector is visible when editing is enabled.</summary>
    public bool ShowInspector { get; init; } = true;

    /// <summary>Gets whether the compact minimap is visible.</summary>
    public bool ShowMinimap { get; init; } = true;

    /// <summary>Gets whether the node-details inspection mode starts enabled.</summary>
    public bool NodeDetailsEnabled { get; init; }

    /// <summary>Gets the host-controlled capabilities that refine the selected interaction mode.</summary>
    public ZoorceDiagramCapabilities? Capabilities { get; init; }

    /// <summary>Gets the largest accepted JSON import in bytes.</summary>
    public int MaximumImportBytes { get; init; } = 1_048_576;

    /// <summary>Gets the pointer gesture used to pan unused canvas space.</summary>
    public ZoorceDiagramPanMode PanMode { get; init; } = ZoorceDiagramPanMode.Background;

    /// <summary>Gets whether nodes snap to the configured grid.</summary>
    public bool SnapToGrid { get; init; } = true;

    /// <summary>Gets the visual grid interval in document units.</summary>
    public int GridSize { get; init; } = 24;

    /// <summary>Gets the lowest permitted zoom level.</summary>
    public decimal MinimumZoom { get; init; } = 0.4m;

    /// <summary>Gets the highest permitted zoom level.</summary>
    public decimal MaximumZoom { get; init; } = 2.4m;

    /// <summary>Gets the initial zoom level.</summary>
    public decimal InitialZoom { get; init; } = 1m;

    /// <summary>Gets the file export capabilities offered by this diagram.</summary>
    public ZoorceDiagramExportOptions? Export { get; init; }

    /// <summary>Validates and normalizes host-supplied diagram configuration.</summary>
    public ZoorceDiagramOptions Normalize(ZoorceDiagramExportOptions? exportDefaults = null)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A diagram requires a unique id.", nameof(Id));
        }

        var minimumZoom = Math.Clamp(MinimumZoom, 0.2m, 1m);
        var maximumZoom = Math.Clamp(MaximumZoom, minimumZoom, 4m);
        var profile = Enum.IsDefined(Profile) ? Profile : ZoorceDiagramProfile.Flowchart;
        var mode = Enum.IsDefined(Mode) ? Mode : ZoorceDiagramMode.Viewer;
        var layout = Enum.IsDefined(Layout) ? Layout : ZoorceDiagramLayout.Manual;
        var panMode = Enum.IsDefined(PanMode) ? PanMode : ZoorceDiagramPanMode.Background;
        var normalizedTitle = string.IsNullOrWhiteSpace(Title) ? "Diagram" : Title.Trim();
        var normalizedSummary = string.IsNullOrWhiteSpace(Summary) ? "Interactive diagram" : Summary.Trim();
        var document = NormalizeDocument(Document ?? new ZoorceDiagramDocument(), profile) with
        {
            Title = TrimOrNull(Document?.Title) ?? normalizedTitle,
            Description = TrimOrNull(Document?.Description) ?? normalizedSummary
        };
        var export = (Export ?? exportDefaults ?? new ZoorceDiagramExportOptions()).Normalize(document.Title!);
        var capabilities = (Capabilities ?? new ZoorceDiagramCapabilities()).Resolve(mode, export.Enabled);

        return this with
        {
            Id = Id.Trim(),
            Title = document.Title!,
            Summary = document.Description!,
            Profile = profile,
            Mode = mode,
            Layout = layout,
            PanMode = panMode,
            Document = document,
            Binding = (Binding ?? new ZoorceDataBindingOptions()).Normalize(),
            SaveUrl = TrimOrNull(SaveUrl),
            ValidateUrl = TrimOrNull(ValidateUrl),
            Capabilities = capabilities,
            MaximumImportBytes = Math.Clamp(MaximumImportBytes, 16_384, 10_485_760),
            GridSize = Math.Clamp(GridSize, 8, 80),
            MinimumZoom = minimumZoom,
            MaximumZoom = maximumZoom,
            InitialZoom = Math.Clamp(InitialZoom, minimumZoom, maximumZoom),
            Export = export
        };
    }

    private static ZoorceDiagramDocument NormalizeDocument(ZoorceDiagramDocument document, ZoorceDiagramProfile profile)
    {
        var nodes = document.NodeList.Select(node =>
        {
            ArgumentNullException.ThrowIfNull(node);
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                throw new ArgumentException("Every diagram node requires an id.", nameof(document));
            }

            ArgumentNullException.ThrowIfNull(node.Position);
            return node with
            {
                Id = node.Id.Trim(),
                Kind = string.IsNullOrWhiteSpace(node.Kind) ? ZoorceDiagramNodeKinds.Process : node.Kind.Trim().ToLowerInvariant(),
                Title = string.IsNullOrWhiteSpace(node.Title) ? "Untitled node" : node.Title.Trim(),
                Identifier = TrimOrNull(node.Identifier),
                Description = TrimOrNull(node.Description),
                Tone = Enum.IsDefined(node.Tone) ? node.Tone : ZoorceDiagramTone.Neutral,
                Action = NormalizeAction(node.Action),
                Style = NormalizeNodeStyle(node.Style)
            };
        }).ToArray();

        if (nodes.Select(node => node.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != nodes.Length)
        {
            throw new ArgumentException("Diagram node ids must be unique.", nameof(document));
        }

        var nodeIds = nodes.Select(node => node.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var edges = document.EdgeList.Select(edge =>
        {
            ArgumentNullException.ThrowIfNull(edge);
            if (string.IsNullOrWhiteSpace(edge.Id) || string.IsNullOrWhiteSpace(edge.FromNodeId) || string.IsNullOrWhiteSpace(edge.ToNodeId))
            {
                throw new ArgumentException("Every diagram edge requires an id, source, and target.", nameof(document));
            }

            var normalized = edge with
            {
                Id = edge.Id.Trim(),
                FromNodeId = edge.FromNodeId.Trim(),
                ToNodeId = edge.ToNodeId.Trim(),
                Label = TrimOrNull(edge.Label),
                Kind = TrimOrNull(edge.Kind),
                Tone = Enum.IsDefined(edge.Tone) ? edge.Tone : ZoorceDiagramTone.Neutral,
                Style = NormalizeEdgeStyle(edge.Style),
                FromPort = Enum.IsDefined(edge.FromPort) ? edge.FromPort : ZoorceDiagramPortSide.Auto,
                ToPort = Enum.IsDefined(edge.ToPort) ? edge.ToPort : ZoorceDiagramPortSide.Auto
            };
            if (!nodeIds.Contains(normalized.FromNodeId) || !nodeIds.Contains(normalized.ToNodeId) || string.Equals(normalized.FromNodeId, normalized.ToNodeId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Diagram edges must connect two different configured nodes.", nameof(document));
            }

            return normalized;
        }).ToArray();

        if (edges.Select(edge => edge.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != edges.Length)
        {
            throw new ArgumentException("Diagram edge ids must be unique.", nameof(document));
        }

        if (edges.Select(edge => edge.FromNodeId + "\u001f" + edge.ToNodeId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != edges.Length)
        {
            throw new ArgumentException("A diagram can only contain one directed connection between the same two nodes.", nameof(document));
        }

        ValidateProfile(profile, nodes, edges);
        return new ZoorceDiagramDocument(Math.Max(1, document.SchemaVersion), nodes, edges, TrimOrNull(document.Version), TrimOrNull(document.Title), TrimOrNull(document.Description));
    }

    private static ZoorceDiagramAction? NormalizeAction(ZoorceDiagramAction? action) => action is null || action.Kind == ZoorceDiagramActionKind.None
        ? null
        : action with
        {
            Kind = Enum.IsDefined(action.Kind) ? action.Kind : ZoorceDiagramActionKind.Event,
            Target = TrimOrNull(action.Target)
        };

    private static ZoorceDiagramNodeStyle? NormalizeNodeStyle(ZoorceDiagramNodeStyle? style)
    {
        if (style is null)
        {
            return null;
        }

        var gradient = style.Gradient is null ? null : style.Gradient with
        {
            StartColor = TrimOrNull(style.Gradient.StartColor) ?? string.Empty,
            EndColor = TrimOrNull(style.Gradient.EndColor) ?? string.Empty,
            Angle = Math.Clamp(style.Gradient.Angle, 0, 360)
        };

        return style with
        {
            FillColor = TrimOrNull(style.FillColor),
            BorderColor = TrimOrNull(style.BorderColor),
            BorderWidth = Clamp(style.BorderWidth, 0.5m, 12m),
            TextColor = TrimOrNull(style.TextColor),
            FontFamily = TrimOrNull(style.FontFamily),
            FontSize = Clamp(style.FontSize, 9m, 42m),
            FontWeight = style.FontWeight is null ? null : Math.Clamp(style.FontWeight.Value, 300, 900),
            CornerRadius = Clamp(style.CornerRadius, 0m, 64m),
            Opacity = Clamp(style.Opacity, 0.15m, 1m),
            Gradient = gradient is { StartColor.Length: > 0, EndColor.Length: > 0 } ? gradient : null
        };
    }

    private static ZoorceDiagramEdgeStyle? NormalizeEdgeStyle(ZoorceDiagramEdgeStyle? style) => style is null
        ? null
        : style with
        {
            StrokeColor = TrimOrNull(style.StrokeColor),
            StrokeWidth = Clamp(style.StrokeWidth, 1m, 8m)
        };

    private static decimal? Clamp(decimal? value, decimal minimum, decimal maximum) => value is null ? null : Math.Clamp(value.Value, minimum, maximum);

    private static void ValidateProfile(ZoorceDiagramProfile profile, IReadOnlyList<ZoorceDiagramNode> nodes, IReadOnlyList<ZoorceDiagramEdge> edges)
    {
        if (profile == ZoorceDiagramProfile.Flowchart)
        {
            if (nodes.Count(node => node.Kind == ZoorceDiagramNodeKinds.Start) > 1 || nodes.Where(node => node.Kind == ZoorceDiagramNodeKinds.End).Any(node => edges.Any(edge => string.Equals(edge.FromNodeId, node.Id, StringComparison.OrdinalIgnoreCase))))
            {
                throw new ArgumentException("A flowchart can have one start and end nodes cannot have outgoing edges.", nameof(nodes));
            }

            return;
        }

        if ((profile == ZoorceDiagramProfile.MindMap || profile == ZoorceDiagramProfile.OrgChart) && nodes.Count > 0)
        {
            var incoming = edges.GroupBy(edge => edge.ToNodeId, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
            if (nodes.Count(node => !incoming.ContainsKey(node.Id)) != 1 || incoming.Values.Any(count => count > 1))
            {
                throw new ArgumentException("Tree diagrams require one root and no node may have more than one parent.", nameof(edges));
            }
        }
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
