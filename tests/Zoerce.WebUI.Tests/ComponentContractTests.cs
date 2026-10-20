using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoerce.WebUI.Abstractions.Data;
using Zoerce.WebUI.Components.Charts;
using Zoerce.WebUI.Components.DataGrid;

namespace Zoerce.WebUI.Tests;

public sealed class ComponentContractTests
{
    [Fact]
    public void Data_contracts_preserve_values_and_normalize_a_window()
    {
        var filters = new Dictionary<string, string?> { ["status"] = "stable" };
        var query = new ZoorceDataQuery(" grid ", "name", true, filters);
        var request = new ZoorcePageRequest { Page = -2, PageSize = 999, Cursor = "  next  " };
        var page = new ZoorceDataPage<string>(["grid"], 7, true, "cursor-2");
        var change = new ZoorceDataChange<string>(ZoorceDataChangeKind.Upsert, "grid", "Data grid");

        var normalized = request.Normalize(50);

        Assert.Equal(" grid ", query.Search);
        Assert.Equal("name", query.Sort);
        Assert.True(query.Descending);
        Assert.Equal("stable", query.Filters!["status"]);
        Assert.Equal(1, normalized.Page);
        Assert.Equal(50, normalized.PageSize);
        Assert.Equal("  next  ", normalized.Cursor);
        Assert.Equal(0, normalized.Offset);
        Assert.Equal("grid", page.Items.Single());
        Assert.Equal(7, page.TotalCount);
        Assert.True(page.HasMore);
        Assert.Equal("cursor-2", page.NextCursor);
        Assert.Equal(ZoorceDataChangeKind.Upsert, change.Kind);
        Assert.Equal("grid", change.Key);
        Assert.Equal("Data grid", change.Item);
    }

    [Fact]
    public void Data_contracts_cover_empty_values_and_all_change_kinds()
    {
        var empty = ZoorceDataPage<int>.Empty();
        var query = new ZoorceDataQuery();
        var request = new ZoorcePageRequest { Page = 3, PageSize = 2, Cursor = "   " }.Normalize();
        var remove = new ZoorceDataChange<string>(ZoorceDataChangeKind.Remove, "one");
        var invalidate = new ZoorceDataChange<string>(ZoorceDataChangeKind.Invalidate, "all");

        Assert.Empty(empty.Items);
        Assert.Null(empty.TotalCount);
        Assert.False(empty.HasMore);
        Assert.Null(empty.NextCursor);
        Assert.Null(query.Search);
        Assert.Null(query.Sort);
        Assert.False(query.Descending);
        Assert.Null(query.Filters);
        Assert.Equal(4, request.Offset);
        Assert.Null(request.Cursor);
        Assert.Null(remove.Item);
        Assert.Equal(ZoorceDataChangeKind.Remove, remove.Kind);
        Assert.Equal(ZoorceDataChangeKind.Invalidate, invalidate.Kind);
    }

    [Fact]
    public void Data_page_request_rejects_invalid_maximum_page_size()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorcePageRequest().Normalize(0));
    }

    [Fact]
    public void Data_grid_normalizes_optional_values_and_bounds_page_size()
    {
        var options = CreateGrid() with
        {
            Id = "  catalogue  ",
            AriaLabel = "  Catalogue  ",
            SourceUrl = " /api/components ",
            RealtimeHubUrl = " /hubs/components ",
            RealtimeEventName = " component-change ",
            PageSize = 1000,
            RowKeyField = " componentId ",
            ViewportHeight = 5000,
            PrefetchThreshold = 5000,
            VirtualizationOverscan = 100,
            RealtimeBehavior = ZoorceDataGridRealtimeBehavior.Patch
        };

        var normalized = options.Normalize();

        Assert.Equal("catalogue", normalized.Id);
        Assert.Equal("Catalogue", normalized.AriaLabel);
        Assert.Equal("/api/components", normalized.SourceUrl);
        Assert.Equal("/hubs/components", normalized.RealtimeHubUrl);
        Assert.Equal("component-change", normalized.RealtimeEventName);
        Assert.Equal(100, normalized.PageSize);
        Assert.Single(normalized.Columns);
        Assert.Empty(normalized.InitialItems);
        Assert.Null(normalized.InitialTotalCount);
        Assert.False(normalized.Virtualize);
        Assert.Equal("componentId", normalized.RowKeyField);
        Assert.Equal(1_200, normalized.ViewportHeight);
        Assert.Equal(1_000, normalized.PrefetchThreshold);
        Assert.Equal(40, normalized.VirtualizationOverscan);
        Assert.Equal(ZoorceDataGridRealtimeBehavior.Patch, normalized.RealtimeBehavior);
    }

    [Fact]
    public void Data_grid_normalizes_blank_optional_values_and_minimum_page_size()
    {
        var normalized = CreateGrid() with
        {
            AriaLabel = " ",
            RealtimeHubUrl = " ",
            RealtimeEventName = " ",
            PageSize = 0,
            RowKeyField = " ",
            ViewportHeight = 0,
            PrefetchThreshold = -1,
            VirtualizationOverscan = -1,
            LoadMode = ZoorceDataLoadMode.InfiniteScroll,
            Virtualize = true,
            InitialItems = [new Dictionary<string, string?> { ["name"] = "Grid" }],
            InitialTotalCount = 1
        };

        normalized = normalized.Normalize();

        Assert.Equal("Data grid", normalized.AriaLabel);
        Assert.Null(normalized.RealtimeHubUrl);
        Assert.Null(normalized.RealtimeEventName);
        Assert.Equal(1, normalized.PageSize);
        Assert.Equal(ZoorceDataLoadMode.InfiniteScroll, normalized.LoadMode);
        Assert.True(normalized.Virtualize);
        Assert.Equal("Grid", normalized.InitialItems.Single()["name"]);
        Assert.Equal(1, normalized.InitialTotalCount);
        Assert.Equal("id", normalized.RowKeyField);
        Assert.Equal(240, normalized.ViewportHeight);
        Assert.Equal(0, normalized.PrefetchThreshold);
        Assert.Equal(0, normalized.VirtualizationOverscan);
        Assert.Equal(ZoorceDataGridRealtimeBehavior.Badge, normalized.RealtimeBehavior);
    }

    [Theory]
    [InlineData("Id")]
    [InlineData("SourceUrl")]
    [InlineData("Columns")]
    public void Data_grid_requires_its_essential_configuration(string missing)
    {
        var options = missing switch
        {
            "Id" => CreateGrid() with { Id = " " },
            "SourceUrl" => CreateGrid() with { SourceUrl = " " },
            _ => CreateGrid() with { Columns = [] }
        };

        Assert.Throws<ArgumentException>(options.Normalize);
    }

    [Fact]
    public void Data_grid_view_component_rejects_null_and_returns_normalized_model()
    {
        var component = new ZoorceDataGridViewComponent();

        Assert.Throws<ArgumentNullException>(() => component.Invoke(null!));
        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateGrid() with { Id = " id " }));
        var model = Assert.IsType<ZoorceDataGridOptions>(result.ViewData!.Model);
        Assert.Equal("id", model.Id);
    }

    [Fact]
    public void Chart_normalizes_optional_values_and_rejects_invalid_required_values()
    {
        var options = CreateChart() with
        {
            Id = " chart ",
            Title = " title ",
            Summary = " summary ",
            ValueSuffix = " % ",
            SourceUrl = " /api/chart ",
            RealtimeHubUrl = " /hub ",
            RealtimeEventName = " update " 
        };
        var normalized = options.Normalize();

        Assert.Equal("chart", normalized.Id);
        Assert.Equal("title", normalized.Title);
        Assert.Equal("summary", normalized.Summary);
        Assert.Equal("%", normalized.ValueSuffix);
        Assert.Equal("/api/chart", normalized.SourceUrl);
        Assert.Equal("/hub", normalized.RealtimeHubUrl);
        Assert.Equal("update", normalized.RealtimeEventName);
        Assert.Equal(ZoorceChartKind.Area, normalized.Kind);
        Assert.Single(normalized.Series);

        Assert.Throws<ArgumentException>(() => (CreateChart() with { Id = " " }).Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceChartOptions { Id = "empty" }.Normalize());
    }

    [Fact]
    public void Chart_normalizes_blanks_and_all_visualization_kinds_are_available()
    {
        var normalized = CreateChart() with
        {
            Title = " ", Summary = " ", ValueSuffix = " ", SourceUrl = " ", RealtimeHubUrl = " ", RealtimeEventName = " ", Kind = ZoorceChartKind.Sparkline, Size = ZoorceChartSize.Compact
        };

        normalized = normalized.Normalize();

        Assert.Equal("Chart", normalized.Title);
        Assert.Equal("Data visualization", normalized.Summary);
        Assert.Null(normalized.SourceUrl);
        Assert.Null(normalized.RealtimeHubUrl);
        Assert.Null(normalized.RealtimeEventName);
        Assert.Null(normalized.ValueSuffix);
        Assert.Equal(ZoorceChartKind.Sparkline, normalized.Kind);
        Assert.Equal(ZoorceChartSize.Compact, normalized.Size);
        Assert.Contains(ZoorceChartKind.Line, Enum.GetValues<ZoorceChartKind>());
        Assert.Contains(ZoorceChartKind.Bar, Enum.GetValues<ZoorceChartKind>());
        Assert.Contains(ZoorceChartKind.Donut, Enum.GetValues<ZoorceChartKind>());
        Assert.Contains(ZoorceChartSize.Wide, Enum.GetValues<ZoorceChartSize>());
    }

    [Fact]
    public void Chart_view_component_rejects_null_and_returns_normalized_model()
    {
        var component = new ZoorceChartViewComponent();

        Assert.Throws<ArgumentNullException>(() => component.Invoke(null!));
        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateChart() with { Id = " visual " }));
        var model = Assert.IsType<ZoorceChartOptions>(result.ViewData!.Model);
        Assert.Equal("visual", model.Id);
    }

    [Fact]
    public void Chart_geometry_normalizes_empty_flat_and_varied_series()
    {
        Assert.Throws<ArgumentNullException>(() => ZoorceChartGeometry.Normalize(null!));
        Assert.Empty(ZoorceChartGeometry.Normalize([]));

        var flat = ZoorceChartGeometry.Normalize([new("One", 4), new("Two", 4)]);
        var varied = ZoorceChartGeometry.Normalize([new("One", 2), new("Two", 6), new("Three", 4)]);

        Assert.Equal((0m, 50m), flat[0]);
        Assert.Equal((100m, 50m), flat[1]);
        Assert.Equal((0m, 100m), varied[0]);
        Assert.Equal((50m, 0m), varied[1]);
        Assert.Equal((100m, 50m), varied[2]);
    }

    private static ZoorceDataGridOptions CreateGrid() => new()
    {
        Id = "catalogue",
        SourceUrl = "/api/components",
        Columns = [new ZoorceDataGridColumn("name", "Name")]
    };

    private static ZoorceChartOptions CreateChart() => new()
    {
        Id = "adoption",
        Title = "Adoption",
        Summary = "Adoption trend",
        Kind = ZoorceChartKind.Area,
        Series = [new ZoorceChartSeries("Series", [new ZoorceChartPoint("Jan", 10)])]
    };
}
