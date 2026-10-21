using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using Zoerce.WebUI.Components.Diagrams;
using Zoerce.WebUI.Options;

namespace Zoerce.WebUI.Tests;

public sealed class DiagramContractTests
{
    [Fact]
    public void Diagram_normalizes_a_complete_configuration()
    {
        var options = CreateFlow() with
        {
            Id = "  release-flow  ",
            Title = "  Release flow  ",
            Summary = "  A process  ",
            SourceUrl = " /api/source ",
            SaveUrl = " /api/save ",
            ValidateUrl = " /api/validate ",
            RealtimeHubUrl = " /hubs/diagrams ",
            RealtimeEventName = " change ",
            NodeDetailsEnabled = true,
            PanMode = ZoorceDiagramPanMode.SpaceKey,
            GridSize = 200,
            MinimumZoom = 0.1m,
            MaximumZoom = 9m,
            InitialZoom = 9m,
            Document = new ZoorceDiagramDocument(0,
            [
                new ZoorceDiagramNode(" start ", " START ", " Start ", new(1, 2), " ", ZoorceDiagramTone.Success, Action: new(ZoorceDiagramActionKind.None, " ignored "), Style: new(" #112233 ", " #445566 ", 20m, " #ffffff ", " mono ", 80m, 950, 90m, 2m, new(" #010203 ", " #abcdef ", 800)), Identifier: "  START-001  "),
                new ZoorceDiagramNode(" work ", " ", " ", new(3, 4), " Detail ", (ZoorceDiagramTone)99, Action: new((ZoorceDiagramActionKind)99, " event-name "), Style: new(Gradient: new(" ", " #abcdef ", -20))),
                new ZoorceDiagramNode(" notify ", " process ", " Notify ", new(5, 6), Action: new(ZoorceDiagramActionKind.Command, " refresh "), Style: new(Gradient: new(" #123456 ", " "))),
                new ZoorceDiagramNode(" default-style ", " process ", " Default style ", new(7, 8), Style: new())
            ],
            [new ZoorceDiagramEdge(" edge ", " start ", " work ", " label ", (ZoorceDiagramTone)99, " kind ", new(" #ddee11 ", 20m, true), (ZoorceDiagramPortSide)99, (ZoorceDiagramPortSide)99)], " version ", "  Release archive  ", "  Release path for approval  ")
        };

        var normalized = options.Normalize();

        Assert.Equal("release-flow", normalized.Id);
        Assert.Equal("Release archive", normalized.Title);
        Assert.Equal("Release path for approval", normalized.Summary);
        Assert.Equal("/api/source", normalized.SourceUrl);
        Assert.Equal("/api/save", normalized.SaveUrl);
        Assert.Equal("/api/validate", normalized.ValidateUrl);
        Assert.Equal("/hubs/diagrams", normalized.RealtimeHubUrl);
        Assert.Equal("change", normalized.RealtimeEventName);
        Assert.True(normalized.NodeDetailsEnabled);
        Assert.Equal(ZoorceDiagramPanMode.SpaceKey, normalized.PanMode);
        Assert.Equal(80, normalized.GridSize);
        Assert.Equal(0.2m, normalized.MinimumZoom);
        Assert.Equal(4m, normalized.MaximumZoom);
        Assert.Equal(4m, normalized.InitialZoom);
        Assert.Equal(1, normalized.Document.SchemaVersion);
        Assert.Equal("start", normalized.Document.NodeList[0].Id);
        Assert.Equal("start", normalized.Document.NodeList[0].Kind);
        Assert.Null(normalized.Document.NodeList[0].Description);
        Assert.Equal("START-001", normalized.Document.NodeList[0].Identifier);
        Assert.Null(normalized.Document.NodeList[0].Action);
        Assert.Equal(ZoorceDiagramNodeKinds.Process, normalized.Document.NodeList[1].Kind);
        Assert.Equal("Untitled node", normalized.Document.NodeList[1].Title);
        Assert.Equal("Detail", normalized.Document.NodeList[1].Description);
        Assert.Equal(ZoorceDiagramTone.Neutral, normalized.Document.NodeList[1].Tone);
        Assert.Equal(ZoorceDiagramActionKind.Event, normalized.Document.NodeList[1].Action!.Kind);
        Assert.Equal("event-name", normalized.Document.NodeList[1].Action!.Target);
        Assert.Equal(ZoorceDiagramActionKind.Command, normalized.Document.NodeList[2].Action!.Kind);
        Assert.Null(normalized.Document.NodeList[2].Style!.Gradient);
        Assert.Null(normalized.Document.NodeList[3].Style!.Gradient);
        Assert.Equal("#112233", normalized.Document.NodeList[0].Style!.FillColor);
        Assert.Equal("#445566", normalized.Document.NodeList[0].Style!.BorderColor);
        Assert.Equal(12m, normalized.Document.NodeList[0].Style!.BorderWidth);
        Assert.Equal("#ffffff", normalized.Document.NodeList[0].Style!.TextColor);
        Assert.Equal("mono", normalized.Document.NodeList[0].Style!.FontFamily);
        Assert.Equal(42m, normalized.Document.NodeList[0].Style!.FontSize);
        Assert.Equal(900, normalized.Document.NodeList[0].Style!.FontWeight);
        Assert.Equal(64m, normalized.Document.NodeList[0].Style!.CornerRadius);
        Assert.Equal(1m, normalized.Document.NodeList[0].Style!.Opacity);
        Assert.Equal(360, normalized.Document.NodeList[0].Style!.Gradient!.Angle);
        Assert.Null(normalized.Document.NodeList[1].Style!.Gradient);
        Assert.Equal("edge", normalized.Document.EdgeList[0].Id);
        Assert.Equal("label", normalized.Document.EdgeList[0].Label);
        Assert.Equal("kind", normalized.Document.EdgeList[0].Kind);
        Assert.Equal(ZoorceDiagramTone.Neutral, normalized.Document.EdgeList[0].Tone);
        Assert.Equal("#ddee11", normalized.Document.EdgeList[0].Style!.StrokeColor);
        Assert.Equal(8m, normalized.Document.EdgeList[0].Style!.StrokeWidth);
        Assert.True(normalized.Document.EdgeList[0].Style!.Dashed);
        Assert.Equal(ZoorceDiagramPortSide.Auto, normalized.Document.EdgeList[0].FromPort);
        Assert.Equal(ZoorceDiagramPortSide.Auto, normalized.Document.EdgeList[0].ToPort);
        Assert.Equal("version", normalized.Document.Version);
        Assert.Equal("Release archive", normalized.Document.Title);
        Assert.Equal("Release path for approval", normalized.Document.Description);
    }

    [Fact]
    public void Diagram_defaults_blank_and_unknown_options_safely()
    {
        var normalized = new ZoorceDiagramOptions
        {
            Id = "diagram",
            Title = " ",
            Summary = " ",
            Profile = (ZoorceDiagramProfile)99,
            Mode = (ZoorceDiagramMode)99,
            Layout = (ZoorceDiagramLayout)99,
            PanMode = (ZoorceDiagramPanMode)99,
            SourceUrl = " ",
            SaveUrl = " ",
            ValidateUrl = " ",
            RealtimeHubUrl = " ",
            RealtimeEventName = " ",
            GridSize = 1,
            MinimumZoom = 2m,
            MaximumZoom = 0.1m,
            InitialZoom = 0.1m,
            Document = null!,
            Export = null!
        }.Normalize();

        Assert.Equal("Diagram", normalized.Title);
        Assert.Equal("Interactive diagram", normalized.Summary);
        Assert.Equal(ZoorceDiagramProfile.Flowchart, normalized.Profile);
        Assert.Equal(ZoorceDiagramMode.Viewer, normalized.Mode);
        Assert.Equal(ZoorceDiagramLayout.Manual, normalized.Layout);
        Assert.Equal(ZoorceDiagramPanMode.Background, normalized.PanMode);
        Assert.False(normalized.NodeDetailsEnabled);
        Assert.Null(normalized.SourceUrl);
        Assert.Null(normalized.SaveUrl);
        Assert.Null(normalized.ValidateUrl);
        Assert.Null(normalized.RealtimeHubUrl);
        Assert.Null(normalized.RealtimeEventName);
        Assert.Equal(8, normalized.GridSize);
        Assert.Equal(1m, normalized.MinimumZoom);
        Assert.Equal(1m, normalized.MaximumZoom);
        Assert.Equal(1m, normalized.InitialZoom);
        Assert.Empty(normalized.Document.NodeList);
        Assert.Empty(normalized.Document.EdgeList);
        Assert.Equal("Diagram", normalized.Document.Title);
        Assert.Equal("Interactive diagram", normalized.Document.Description);
        Assert.True(normalized.Export!.Enabled);
        Assert.Equal(ZoorceDiagramExportFormat.Svg, normalized.Export.DefaultFormat);
    }

    [Fact]
    public void Diagram_rejects_missing_and_duplicate_document_parts()
    {
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Id = " " }).Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateFlow() with { Document = new(1, [null!]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node(" ")]) }).Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateFlow() with { Document = new(1, [Node("one") with { Position = null! }]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one"), Node(" ONE ")]) }).Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateFlow() with { Document = new(1, [Node("one")], [null!]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one")], [new(" ", "one", "one")]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one")], [new("edge", "one", "missing")]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one"), Node("two")], [new("edge", "one", "two"), new(" EDGE ", "two", "one")]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one"), Node("two")], [new("edge-one", "one", "two"), new("edge-two", "two", "one"), new("edge-three", "one", "two")]) }).Normalize());
    }

    [Fact]
    public void Diagram_enforces_flow_and_tree_grammars()
    {
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("one", ZoorceDiagramNodeKinds.Start), Node("two", ZoorceDiagramNodeKinds.Start)]) }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateFlow() with { Document = new(1, [Node("end", ZoorceDiagramNodeKinds.End), Node("next")], [new("edge", "end", "next")]) }).Normalize());
        Assert.Throws<ArgumentException>(() => CreateTree(ZoorceDiagramProfile.MindMap, [Node("a"), Node("b")], []).Normalize());
        Assert.Throws<ArgumentException>(() => CreateTree(ZoorceDiagramProfile.OrgChart, [Node("root"), Node("left"), Node("right"), Node("child")], [new("one", "root", "child"), new("two", "left", "child"), new("three", "right", "left")]).Normalize());

        var mind = CreateTree(ZoorceDiagramProfile.MindMap, [Node("root"), Node("child")], [new("one", "root", "child")]).Normalize();
        var emptyTree = CreateTree(ZoorceDiagramProfile.OrgChart, [], []).Normalize();
        var architecture = CreateFlow() with { Profile = ZoorceDiagramProfile.Architecture, Document = new(1, [Node("a"), Node("b")]) };

        Assert.Equal(2, mind.Document.NodeList.Count);
        Assert.Empty(emptyTree.Document.NodeList);
        Assert.Equal(ZoorceDiagramProfile.Architecture, architecture.Normalize().Profile);
    }

    [Fact]
    public void Diagram_contracts_and_view_component_are_available()
    {
        var empty = new ZoorceDiagramDocument();
        var values = new ZoorceDiagramDocument(Nodes: [Node("one") with { Collapsed = true }], Edges: [new("edge", "one", "one")]);
        var component = new ZoorceDiagramViewComponent();

        Assert.Empty(empty.NodeList);
        Assert.Empty(empty.EdgeList);
        Assert.Single(values.NodeList);
        Assert.Single(values.EdgeList);
        Assert.True(values.NodeList[0].Collapsed);
        Assert.Null(values.NodeList[0].Data);
        Assert.Null(values.NodeList[0].Identifier);
        Assert.Contains(ZoorceDiagramProfile.Architecture, Enum.GetValues<ZoorceDiagramProfile>());
        Assert.Contains(ZoorceDiagramMode.ReadOnly, Enum.GetValues<ZoorceDiagramMode>());
        Assert.Contains(ZoorceDiagramLayout.TreeHorizontal, Enum.GetValues<ZoorceDiagramLayout>());
        Assert.Contains(ZoorceDiagramPanMode.MiddleButton, Enum.GetValues<ZoorceDiagramPanMode>());
        Assert.Contains(ZoorceDiagramPortSide.Bottom, Enum.GetValues<ZoorceDiagramPortSide>());
        Assert.Contains(ZoorceDiagramTone.Danger, Enum.GetValues<ZoorceDiagramTone>());
        Assert.Contains(ZoorceDiagramActionKind.Endpoint, Enum.GetValues<ZoorceDiagramActionKind>());
        Assert.Equal("#010203", new ZoorceDiagramGradient("#010203", "#040506").StartColor);
        Assert.Equal(ZoorceDiagramNodeKinds.DataStore, ZoorceDiagramNodeKinds.DataStore);
        Assert.Throws<ArgumentNullException>(() => component.Invoke(null!));

        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateFlow() with { Id = " diagram-view " }));
        var model = Assert.IsType<ZoorceDiagramOptions>(result.ViewData!.Model);
        Assert.Equal("diagram-view", model.Id);
    }

    [Fact]
    public void Diagram_normalizes_export_configuration_and_pdf_modes()
    {
        var normalized = (CreateFlow() with
        {
            Title = "  Architecture overview  ",
            Export = new ZoorceDiagramExportOptions
            {
                AllowedFormats = [ZoorceDiagramExportFormat.Svg, ZoorceDiagramExportFormat.Svg, (ZoorceDiagramExportFormat)99],
                DefaultFormat = ZoorceDiagramExportFormat.Pdf,
                FileName = "  platform-map  ",
                DefaultPadding = 999,
                JpegScales = [0, 2, 2, 7],
                DefaultJpegScale = 3,
                DefaultBackground = (ZoorceDiagramExportBackground)99,
                SolidBackground = "  #110022  ",
                PdfMode = ZoorceDiagramPdfMode.Server,
                PdfExportUrl = " /api/export/pdf "
            }
        }).Normalize();

        Assert.Equal([ZoorceDiagramExportFormat.Svg], normalized.Export!.AllowedFormats);
        Assert.Equal(ZoorceDiagramExportFormat.Svg, normalized.Export.DefaultFormat);
        Assert.Equal("platform-map", normalized.Export.FileName);
        Assert.Equal(240, normalized.Export.DefaultPadding);
        Assert.Equal([2], normalized.Export.JpegScales);
        Assert.Equal(2, normalized.Export.DefaultJpegScale);
        Assert.Equal(ZoorceDiagramExportBackground.Transparent, normalized.Export.DefaultBackground);
        Assert.Equal("#110022", normalized.Export.SolidBackground);
        Assert.Equal(ZoorceDiagramPdfMode.Server, normalized.Export.PdfMode);
        Assert.Equal("/api/export/pdf", normalized.Export.PdfExportUrl);

        var fallback = (CreateFlow() with
        {
            Export = new ZoorceDiagramExportOptions
            {
                AllowedFormats = [(ZoorceDiagramExportFormat)99],
                DefaultFormat = (ZoorceDiagramExportFormat)99,
                FileName = " ",
                JpegScales = [0, 5],
                DefaultJpegScale = 8,
                PdfMode = ZoorceDiagramPdfMode.Server,
                PdfExportUrl = " "
            }
        }).Normalize();

        Assert.Equal(5, fallback.Export!.AllowedFormats!.Count);
        Assert.Equal(ZoorceDiagramExportFormat.Svg, fallback.Export.DefaultFormat);
        Assert.Equal("Flow", fallback.Export.FileName);
        Assert.Equal([1, 2, 3], fallback.Export.JpegScales);
        Assert.Equal(3, fallback.Export.DefaultJpegScale);
        Assert.Equal(ZoorceDiagramPdfMode.BrowserPrint, fallback.Export.PdfMode);
        Assert.Null(fallback.Export.PdfExportUrl);

        var nullCollections = new ZoorceDiagramExportOptions
        {
            AllowedFormats = null,
            JpegScales = null,
            FileName = " "
        }.Normalize(" ");
        Assert.Equal(5, nullCollections.AllowedFormats!.Count);
        Assert.Equal([1, 2, 3], nullCollections.JpegScales);
        Assert.Equal("diagram", nullCollections.FileName);

        var defaults = new ZoerceWebUiOptions
        {
            DiagramExport = new ZoorceDiagramExportOptions { Enabled = false, FileName = "global" }
        };
        var component = new ZoorceDiagramViewComponent(Microsoft.Extensions.Options.Options.Create(defaults));
        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateFlow()));
        var model = Assert.IsType<ZoorceDiagramOptions>(result.ViewData!.Model);
        Assert.False(model.Export!.Enabled);
        Assert.Equal("global", model.Export.FileName);
    }

    private static ZoorceDiagramOptions CreateFlow() => new()
    {
        Id = "flow",
        Title = "Flow",
        Summary = "A flow",
        Profile = ZoorceDiagramProfile.Flowchart,
        Document = new(1, [Node("start", ZoorceDiagramNodeKinds.Start), Node("work")], [new("edge", "start", "work")])
    };

    private static ZoorceDiagramOptions CreateTree(ZoorceDiagramProfile profile, IReadOnlyList<ZoorceDiagramNode> nodes, IReadOnlyList<ZoorceDiagramEdge> edges) => new()
    {
        Id = "tree",
        Profile = profile,
        Document = new(1, nodes, edges)
    };

    private static ZoorceDiagramNode Node(string id, string kind = ZoorceDiagramNodeKinds.Process)
        => new(id, kind, id, new(10, 20));
}
