using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoerce.WebUI.Abstractions.Data;
using Zoerce.WebUI.Components.Charts;
using Zoerce.WebUI.Components.DataGrid;
using Zoerce.WebUI.Components.Forms;
using Zoerce.WebUI.Components.Stepper;
using Zoerce.WebUI.Components.Timeline;

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
    public void Chart_supports_visual_options_and_rejects_an_inverted_value_range()
    {
        var normalized = CreateChart() with
        {
            Kind = ZoorceChartKind.Bullet,
            BarMode = ZoorceChartBarMode.Stacked100,
            Curve = ZoorceChartCurve.Step,
            ShowLegend = false,
            ShowGrid = false,
            ShowAxes = false,
            ShowTooltip = false,
            MinimumValue = 0,
            MaximumValue = 100,
            TargetValue = 72
        };

        normalized = normalized.Normalize();

        Assert.Equal(ZoorceChartKind.Bullet, normalized.Kind);
        Assert.Equal(ZoorceChartBarMode.Stacked100, normalized.BarMode);
        Assert.Equal(ZoorceChartCurve.Step, normalized.Curve);
        Assert.False(normalized.ShowLegend);
        Assert.False(normalized.ShowGrid);
        Assert.False(normalized.ShowAxes);
        Assert.False(normalized.ShowTooltip);
        Assert.Equal(0, normalized.MinimumValue);
        Assert.Equal(100, normalized.MaximumValue);
        Assert.Equal(72, normalized.TargetValue);
        Assert.Equal(ZoorceChartKind.Gauge, Enum.GetValues<ZoorceChartKind>().Single(value => value == ZoorceChartKind.Gauge));
        Assert.Equal(ZoorceChartKind.Waterfall, Enum.GetValues<ZoorceChartKind>().Single(value => value == ZoorceChartKind.Waterfall));

        Assert.Equal(0, (CreateChart() with { MinimumValue = 0 }).Normalize().MinimumValue);
        Assert.Throws<ArgumentException>(() => (CreateChart() with { MinimumValue = 100, MaximumValue = 0 }).Normalize());
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

    [Fact]
    public void Chart_points_preserve_optional_dimensions_for_specialized_visualizations()
    {
        var point = new ZoorceChartPoint("Account", 42, 18, 4, 52, 12, "North");
        var series = new ZoorceChartSeries("Opportunity", [point], "#7c3aed");

        Assert.Equal("Account", point.Label);
        Assert.Equal(42, point.Value);
        Assert.Equal(18, point.X);
        Assert.Equal(4, point.Low);
        Assert.Equal(52, point.High);
        Assert.Equal(12, point.Size);
        Assert.Equal("North", point.Group);
        Assert.Equal("Opportunity", series.Name);
        Assert.Same(point, series.Points.Single());
        Assert.Equal("#7c3aed", series.Color);
    }

    [Fact]
    public void Stepper_normalizes_a_configurable_workflow_and_forces_supported_variant_orientations()
    {
        var normalized = CreateStepper() with
        {
            Id = "  release-flow  ",
            AriaLabel = "  ",
            CurrentStepId = "  validate  ",
            Variant = ZoorceStepperVariant.Segmented,
            Orientation = ZoorceStepperOrientation.Vertical,
            PreviousLabel = " ",
            NextLabel = " ",
            CompleteLabel = " ",
            SourceUrl = " /api/releases ",
            RealtimeHubUrl = " /hubs/releases ",
            RealtimeEventName = " release-update ",
            RealtimeTransport = ZoorceStepperRealtimeTransport.LongPolling,
            Steps =
            [
                new ZoorceStepperStep(" brief ", " ", " context ", " ✓ ", ZoorceStepperState.Completed, " done ", TargetUrl: " /brief ", PanelId: " brief-panel "),
                new ZoorceStepperStep(" validate ", " Validate ", " ", null, ZoorceStepperState.Active, null, PanelId: " ")
            ]
        };

        normalized = normalized.Normalize();

        Assert.Equal("release-flow", normalized.Id);
        Assert.Equal("Workflow progress", normalized.AriaLabel);
        Assert.Equal("validate", normalized.CurrentStepId);
        Assert.Equal(ZoorceStepperOrientation.Horizontal, normalized.Orientation);
        Assert.Equal("Back", normalized.PreviousLabel);
        Assert.Equal("Continue", normalized.NextLabel);
        Assert.Equal("Complete", normalized.CompleteLabel);
        Assert.Equal("/api/releases", normalized.SourceUrl);
        Assert.Equal("/hubs/releases", normalized.RealtimeHubUrl);
        Assert.Equal("release-update", normalized.RealtimeEventName);
        Assert.Equal(ZoorceStepperRealtimeTransport.LongPolling, normalized.RealtimeTransport);
        Assert.Equal("brief", normalized.Steps[0].Id);
        Assert.Equal("Step", normalized.Steps[0].Title);
        Assert.Equal("context", normalized.Steps[0].SupportingText);
        Assert.Equal("✓", normalized.Steps[0].Icon);
        Assert.Equal("done", normalized.Steps[0].StatusText);
        Assert.Equal("/brief", normalized.Steps[0].TargetUrl);
        Assert.Equal("brief-panel", normalized.Steps[0].PanelId);
        Assert.Null(normalized.Steps[1].SupportingText);
        Assert.Null(normalized.Steps[1].Icon);
        Assert.Null(normalized.Steps[1].StatusText);
        Assert.Null(normalized.Steps[1].PanelId);

        var flatSegments = (CreateStepper() with
        {
            Variant = ZoorceStepperVariant.FlatSegments,
            Orientation = ZoorceStepperOrientation.Vertical
        }).Normalize();
        var wizardHeader = (CreateStepper() with
        {
            Variant = ZoorceStepperVariant.WizardHeader,
            Orientation = ZoorceStepperOrientation.Vertical
        }).Normalize();

        Assert.Equal(ZoorceStepperOrientation.Horizontal, flatSegments.Orientation);
        Assert.Equal(ZoorceStepperOrientation.Horizontal, wizardHeader.Orientation);
    }

    [Fact]
    public void Stepper_selects_active_then_available_step_and_keeps_explicit_labels()
    {
        var active = (CreateStepper() with
        {
            CurrentStepId = " ",
            Steps =
            [
                new ZoorceStepperStep("brief", "Brief", State: ZoorceStepperState.Completed),
                new ZoorceStepperStep("validate", "Validate", State: ZoorceStepperState.Active)
            ]
        }).Normalize();
        var available = (CreateStepper() with
        {
            CurrentStepId = null,
            Variant = ZoorceStepperVariant.Sidebar,
            Orientation = ZoorceStepperOrientation.Horizontal,
            Steps =
            [
                new ZoorceStepperStep("locked", "Locked", State: ZoorceStepperState.Disabled),
                new ZoorceStepperStep("brief", "Brief")
            ]
        }).Normalize();

        Assert.Equal("validate", active.CurrentStepId);
        Assert.Equal(ZoorceStepperInteraction.Progress, active.Interaction);
        Assert.Equal(ZoorceStepperDensity.Comfortable, active.Density);
        Assert.False(active.AllowFutureNavigation);
        Assert.Equal("Previous", active.PreviousLabel);
        Assert.Equal("Continue now", active.NextLabel);
        Assert.Equal("Finish", active.CompleteLabel);
        Assert.Null(active.SourceUrl);
        Assert.Null(active.RealtimeHubUrl);
        Assert.Null(active.RealtimeEventName);
        Assert.Equal("brief", available.CurrentStepId);
        Assert.Equal(ZoorceStepperOrientation.Vertical, available.Orientation);
    }

    [Fact]
    public void Stepper_normalizes_presentation_transition_rules_and_semantic_tones()
    {
        var normalized = (CreateStepper() with
        {
            TransitionUrl = " /api/stepper/authorize ",
            Presentation = new ZoorceStepperPresentation
            {
                MarkerStyle = ZoorceStepperMarkerStyle.Icon,
                ActiveIndicator = ZoorceStepperActiveIndicator.Halo,
                ActiveMotion = ZoorceStepperActiveMotion.RingPulse,
                LabelPlacement = ZoorceStepperLabelPlacement.End,
                ConnectorStyle = ZoorceStepperConnectorStyle.Dotted
            },
            TransitionRules = new ZoorceStepperTransitionRules
            {
                Forward = ZoorceStepperForwardRule.AdjacentOnly,
                Backward = ZoorceStepperBackwardRule.CompletedOnly,
                Warning = ZoorceStepperWarningRule.RequireAuthorization,
                Error = ZoorceStepperErrorRule.RequireAuthorization,
                OnBackward = ZoorceStepperBackwardEffect.ResetFollowing
            },
            Steps =
            [
                new ZoorceStepperStep("brief", "Brief", Tone: ZoorceStepperTone.Success, State: ZoorceStepperState.Active),
                new ZoorceStepperStep("validate", "Validate", Tone: ZoorceStepperTone.Info)
            ]
        }).Normalize();

        Assert.Equal("/api/stepper/authorize", normalized.TransitionUrl);
        Assert.Equal(ZoorceStepperMarkerStyle.Icon, normalized.Presentation.MarkerStyle);
        Assert.Equal(ZoorceStepperActiveIndicator.Halo, normalized.Presentation.ActiveIndicator);
        Assert.Equal(ZoorceStepperActiveMotion.RingPulse, normalized.Presentation.ActiveMotion);
        Assert.Equal(ZoorceStepperLabelPlacement.End, normalized.Presentation.LabelPlacement);
        Assert.Equal(ZoorceStepperConnectorStyle.Dotted, normalized.Presentation.ConnectorStyle);
        Assert.Equal(ZoorceStepperForwardRule.AdjacentOnly, normalized.TransitionRules.Forward);
        Assert.Equal(ZoorceStepperBackwardRule.CompletedOnly, normalized.TransitionRules.Backward);
        Assert.Equal(ZoorceStepperWarningRule.RequireAuthorization, normalized.TransitionRules.Warning);
        Assert.Equal(ZoorceStepperErrorRule.RequireAuthorization, normalized.TransitionRules.Error);
        Assert.Equal(ZoorceStepperBackwardEffect.ResetFollowing, normalized.TransitionRules.OnBackward);
        Assert.Equal(ZoorceStepperTone.Success, normalized.Steps[0].Tone);
        Assert.Equal(ZoorceStepperTone.Info, normalized.Steps[1].Tone);
    }

    [Fact]
    public void Stepper_replaces_unknown_presentation_rules_and_tones_with_safe_defaults()
    {
        var normalized = (CreateStepper() with
        {
            Presentation = new ZoorceStepperPresentation
            {
                MarkerStyle = (ZoorceStepperMarkerStyle)999,
                ActiveIndicator = (ZoorceStepperActiveIndicator)999,
                ActiveMotion = (ZoorceStepperActiveMotion)999,
                LabelPlacement = (ZoorceStepperLabelPlacement)999,
                ConnectorStyle = (ZoorceStepperConnectorStyle)999
            },
            TransitionRules = new ZoorceStepperTransitionRules
            {
                Forward = (ZoorceStepperForwardRule)999,
                Backward = (ZoorceStepperBackwardRule)999,
                Warning = (ZoorceStepperWarningRule)999,
                Error = (ZoorceStepperErrorRule)999,
                OnBackward = (ZoorceStepperBackwardEffect)999
            },
            RealtimeTransport = (ZoorceStepperRealtimeTransport)999,
            Steps =
            [
                new ZoorceStepperStep("brief", "Brief", State: ZoorceStepperState.Active, Tone: (ZoorceStepperTone)999),
                new ZoorceStepperStep("validate", "Validate")
            ]
        }).Normalize();
        var withNullContracts = (CreateStepper() with
        {
            Presentation = null!,
            TransitionRules = null!,
            Steps =
            [
                new ZoorceStepperStep("brief", "Brief", State: ZoorceStepperState.Active),
                new ZoorceStepperStep("validate", "Validate")
            ]
        }).Normalize();

        Assert.Equal(ZoorceStepperMarkerStyle.Auto, normalized.Presentation.MarkerStyle);
        Assert.Equal(ZoorceStepperActiveIndicator.Ring, normalized.Presentation.ActiveIndicator);
        Assert.Equal(ZoorceStepperActiveMotion.None, normalized.Presentation.ActiveMotion);
        Assert.Equal(ZoorceStepperLabelPlacement.Auto, normalized.Presentation.LabelPlacement);
        Assert.Equal(ZoorceStepperConnectorStyle.Solid, normalized.Presentation.ConnectorStyle);
        Assert.Equal(ZoorceStepperForwardRule.Auto, normalized.TransitionRules.Forward);
        Assert.Equal(ZoorceStepperBackwardRule.Auto, normalized.TransitionRules.Backward);
        Assert.Equal(ZoorceStepperWarningRule.Allow, normalized.TransitionRules.Warning);
        Assert.Equal(ZoorceStepperErrorRule.Block, normalized.TransitionRules.Error);
        Assert.Equal(ZoorceStepperBackwardEffect.Auto, normalized.TransitionRules.OnBackward);
        Assert.Equal(ZoorceStepperTone.Auto, normalized.Steps[0].Tone);
        Assert.Equal(ZoorceStepperRealtimeTransport.Auto, normalized.RealtimeTransport);
        Assert.Equal(new ZoorceStepperPresentation(), withNullContracts.Presentation);
        Assert.Equal(new ZoorceStepperTransitionRules(), withNullContracts.TransitionRules);
    }

    [Fact]
    public void Stepper_transition_contract_preserves_authorization_payloads()
    {
        var request = new ZoorceStepperTransitionRequest("policy", "approval", "risk", "next", false);
        var step = new ZoorceStepperStepSnapshot("approval", ZoorceStepperState.Active, ZoorceStepperTone.Info, "Approval", "Owner confirmation", "In progress", "⌁", true, "/approve");
        var snapshot = new ZoorceStepperSnapshot("approval", [step]);
        var accepted = new ZoorceStepperTransitionDecision(true, "Authorized", snapshot);
        var rejected = new ZoorceStepperTransitionDecision(false);

        Assert.Equal("policy", request.StepperId);
        Assert.Equal("approval", request.StepId);
        Assert.Equal("risk", request.PreviousStepId);
        Assert.Equal("next", request.Reason);
        Assert.False(request.IsBackward);
        Assert.True(accepted.Allowed);
        Assert.Equal("Authorized", accepted.Message);
        Assert.Same(snapshot, accepted.Snapshot);
        Assert.False(rejected.Allowed);
        Assert.Null(rejected.Message);
        Assert.Equal("approval", snapshot.CurrentStepId);
        Assert.Same(step, snapshot.Steps.Single());
        Assert.Equal("approval", step.Id);
        Assert.Equal(ZoorceStepperState.Active, step.State);
        Assert.Equal(ZoorceStepperTone.Info, step.Tone);
        Assert.Equal("Approval", step.Title);
        Assert.Equal("Owner confirmation", step.SupportingText);
        Assert.Equal("In progress", step.StatusText);
        Assert.Equal("⌁", step.Icon);
        Assert.True(step.IsNavigable);
        Assert.Equal("/approve", step.TargetUrl);
    }

    [Fact]
    public void Stepper_rejects_invalid_configuration_and_all_disabled_workflows()
    {
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { Id = " " }).Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceStepperOptions { Id = "one", Steps = [new ZoorceStepperStep("only", "Only")] }.Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateStepper() with { Steps = [null!, new ZoorceStepperStep("two", "Two")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { Steps = [new ZoorceStepperStep(" ", "One"), new ZoorceStepperStep("two", "Two")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { Steps = [new ZoorceStepperStep("same", "One"), new ZoorceStepperStep(" SAME ", "Two")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { CurrentStepId = "missing" }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { CurrentStepId = "brief", Steps = [new ZoorceStepperStep("brief", "Brief", State: ZoorceStepperState.Disabled), new ZoorceStepperStep("validate", "Validate")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateStepper() with { CurrentStepId = null, Steps = [new ZoorceStepperStep("one", "One", State: ZoorceStepperState.Disabled), new ZoorceStepperStep("two", "Two", State: ZoorceStepperState.Disabled)] }).Normalize());
    }

    [Fact]
    public void Stepper_contracts_and_view_component_are_available()
    {
        var step = new ZoorceStepperStep("welcome", "Welcome", IsOptional: true, IsNavigable: false, TargetUrl: "/welcome", PanelId: "welcome-panel");
        var component = new ZoorceStepperViewComponent();

        Assert.Equal("welcome", step.Id);
        Assert.True(step.IsOptional);
        Assert.False(step.IsNavigable);
        Assert.Equal("/welcome", step.TargetUrl);
        Assert.Equal("welcome-panel", step.PanelId);
        Assert.Throws<ArgumentNullException>(() => component.Invoke(null!));
        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateStepper() with { Id = " stepper-view " }));
        var model = Assert.IsType<ZoorceStepperOptions>(result.ViewData!.Model);
        Assert.Equal("stepper-view", model.Id);
        Assert.Contains(ZoorceStepperVariant.Workflow, Enum.GetValues<ZoorceStepperVariant>());
        Assert.Contains(ZoorceStepperVariant.NumberedRail, Enum.GetValues<ZoorceStepperVariant>());
        Assert.Contains(ZoorceStepperVariant.FlatSegments, Enum.GetValues<ZoorceStepperVariant>());
        Assert.Contains(ZoorceStepperVariant.WizardHeader, Enum.GetValues<ZoorceStepperVariant>());
        Assert.Contains(ZoorceStepperVariant.MinimalRail, Enum.GetValues<ZoorceStepperVariant>());
        Assert.Contains(ZoorceStepperOrientation.Auto, Enum.GetValues<ZoorceStepperOrientation>());
        Assert.Contains(ZoorceStepperInteraction.Wizard, Enum.GetValues<ZoorceStepperInteraction>());
        Assert.Contains(ZoorceStepperDensity.Compact, Enum.GetValues<ZoorceStepperDensity>());
        Assert.Contains(ZoorceStepperState.Error, Enum.GetValues<ZoorceStepperState>());
        Assert.Contains(ZoorceStepperMarkerStyle.Dot, Enum.GetValues<ZoorceStepperMarkerStyle>());
        Assert.Contains(ZoorceStepperActiveMotion.ConnectorDraw, Enum.GetValues<ZoorceStepperActiveMotion>());
        Assert.Contains(ZoorceStepperForwardRule.Block, Enum.GetValues<ZoorceStepperForwardRule>());
        Assert.Contains(ZoorceStepperTone.Danger, Enum.GetValues<ZoorceStepperTone>());
        Assert.Contains(ZoorceStepperRealtimeTransport.ServerSentEvents, Enum.GetValues<ZoorceStepperRealtimeTransport>());
    }

    [Fact]
    public void Timeline_normalizes_items_presentation_and_explicit_overflow_behavior()
    {
        var normalized = (CreateTimeline() with
        {
            Id = " timeline ",
            AriaLabel = " ",
            SelectedItemId = "later",
            SortAscending = false,
            ShowRelativeTime = true,
            FollowLatest = true,
            Variant = ZoorceTimelineVariant.Milestones,
            Orientation = ZoorceTimelineOrientation.Vertical,
            OverflowMode = ZoorceTimelineOverflowMode.Stack,
            ShowOverflowControls = false,
            MaxItems = 4,
            SourceUrl = " /api/timeline ",
            RealtimeHubUrl = " /hubs/timeline ",
            RealtimeEventName = " timeline-update ",
            RealtimeTransport = ZoorceTimelineRealtimeTransport.LongPolling,
            Presentation = new ZoorceTimelinePresentation
            {
                MarkerStyle = ZoorceTimelineMarkerStyle.Icon,
                ConnectorStyle = ZoorceTimelineConnectorStyle.Dashed,
                ActiveMotion = ZoorceTimelineActiveMotion.HaloPulse,
                UseCards = false
            },
            Items =
            [
                new ZoorceTimelineItem(" later ", DateTimeOffset.Parse("2026-10-22T12:00:00+00:00"), " ", " details ", " ✦ ", ZoorceTimelineTone.Success, ZoorceTimelineState.Completed, ZoorceTimelinePlacement.Left, " launch ", new ZoorceTimelineActor(" Ada ", " A ", " /ada.png ", " Owner "), [" stable ", "STABLE", " " ], new Dictionary<string, string> { [" Region "] = " us-east ", [" "] = "ignored", ["Empty"] = " " }, " 10 min ", " studio ", [new ZoorceTimelineActor(" Bea ", " B ")], new ZoorceTimelineAction(" open ", " Open item ", " ↗ ", " /open "), false, true),
                new ZoorceTimelineItem(" earlier ", DateTimeOffset.Parse("2026-10-20T12:00:00+00:00"), "Earlier")
            ]
        }).Normalize();

        Assert.Equal("timeline", normalized.Id);
        Assert.Equal("Timeline", normalized.AriaLabel);
        Assert.Equal(ZoorceTimelineOrientation.Vertical, normalized.Orientation);
        Assert.Equal(ZoorceTimelineOverflowMode.Stack, normalized.OverflowMode);
        Assert.False(normalized.ShowOverflowControls);
        Assert.Equal("later", normalized.Items[0].Id);
        Assert.Equal("Event", normalized.Items[0].Title);
        Assert.Equal("details", normalized.Items[0].Description);
        Assert.Equal("✦", normalized.Items[0].Icon);
        Assert.Equal("launch", normalized.Items[0].Flag);
        Assert.Equal("Ada", normalized.Items[0].Actor!.Name);
        Assert.Equal("A", normalized.Items[0].Actor!.Initials);
        Assert.Equal("Owner", normalized.Items[0].Actor!.Role);
        Assert.Equal(new[] { "stable" }, normalized.Items[0].Tags);
        Assert.Equal("us-east", normalized.Items[0].Metadata!["Region"]);
        Assert.Equal("Bea", normalized.Items[0].Participants!.Single().Name);
        Assert.Equal("open", normalized.Items[0].Action!.Id);
        Assert.Equal(4, normalized.MaxItems);
        Assert.True(normalized.ShowRelativeTime);
        Assert.True(normalized.FollowLatest);
        Assert.Equal("/api/timeline", normalized.SourceUrl);
        Assert.Equal("/hubs/timeline", normalized.RealtimeHubUrl);
        Assert.Equal("timeline-update", normalized.RealtimeEventName);
        Assert.Equal(ZoorceTimelineRealtimeTransport.LongPolling, normalized.RealtimeTransport);
        Assert.Equal(ZoorceTimelineMarkerStyle.Icon, normalized.Presentation.MarkerStyle);
        Assert.False(normalized.Presentation.UseCards);
    }

    [Fact]
    public void Timeline_uses_safe_defaults_for_invalid_optional_configuration()
    {
        var normalized = (CreateTimeline() with
        {
            Variant = (ZoorceTimelineVariant)999,
            SelectedItemId = "one",
            Orientation = (ZoorceTimelineOrientation)999,
            OverflowMode = (ZoorceTimelineOverflowMode)999,
            Interaction = (ZoorceTimelineInteraction)999,
            Density = (ZoorceTimelineDensity)999,
            GroupBy = (ZoorceTimelineGroupBy)999,
            RefreshMode = (ZoorceTimelineRefreshMode)999,
            RealtimeTransport = (ZoorceTimelineRealtimeTransport)999,
            MaxItems = 0,
            EmptyMessage = " ",
            Presentation = new ZoorceTimelinePresentation
            {
                MarkerStyle = (ZoorceTimelineMarkerStyle)999,
                ConnectorStyle = (ZoorceTimelineConnectorStyle)999,
                ActiveMotion = (ZoorceTimelineActiveMotion)999
            },
            Items =
            [
                new ZoorceTimelineItem("one", DateTimeOffset.UtcNow, "One", Tone: (ZoorceTimelineTone)999, State: (ZoorceTimelineState)999, Placement: (ZoorceTimelinePlacement)999, Actor: new ZoorceTimelineActor(" "), Action: new ZoorceTimelineAction(" ", " ")),
                new ZoorceTimelineItem("two", DateTimeOffset.UtcNow.AddMinutes(1), "Two", Tags: null, Metadata: null, Participants: null)
            ]
        }).Normalize();

        Assert.Equal(ZoorceTimelineVariant.Activity, normalized.Variant);
        Assert.Equal(ZoorceTimelineOrientation.Auto, normalized.Orientation);
        Assert.Equal(ZoorceTimelineOverflowMode.Auto, normalized.OverflowMode);
        Assert.Equal(ZoorceTimelineInteraction.ReadOnly, normalized.Interaction);
        Assert.Equal(ZoorceTimelineDensity.Default, normalized.Density);
        Assert.Equal(ZoorceTimelineGroupBy.None, normalized.GroupBy);
        Assert.Equal(ZoorceTimelineRefreshMode.Merge, normalized.RefreshMode);
        Assert.Equal(ZoorceTimelineRealtimeTransport.Auto, normalized.RealtimeTransport);
        Assert.Null(normalized.MaxItems);
        Assert.Equal("No events to display.", normalized.EmptyMessage);
        Assert.Equal(ZoorceTimelineMarkerStyle.Auto, normalized.Presentation.MarkerStyle);
        Assert.Equal(ZoorceTimelineConnectorStyle.Solid, normalized.Presentation.ConnectorStyle);
        Assert.Equal(ZoorceTimelineActiveMotion.None, normalized.Presentation.ActiveMotion);
        Assert.Equal(ZoorceTimelineTone.Auto, normalized.Items[0].Tone);
        Assert.Equal(ZoorceTimelineState.Default, normalized.Items[0].State);
        Assert.Equal(ZoorceTimelinePlacement.Auto, normalized.Items[0].Placement);
        Assert.Null(normalized.Items[0].Actor);
        Assert.Null(normalized.Items[0].Action);
        Assert.Empty(normalized.Items[1].Tags!);
        Assert.Empty(normalized.Items[1].Metadata!);
        Assert.Empty(normalized.Items[1].Participants!);

        var withNullPresentation = (CreateTimeline() with { Presentation = null! }).Normalize();
        Assert.Equal(new ZoorceTimelinePresentation(), withNullPresentation.Presentation);
    }

    [Fact]
    public void Timeline_normalizes_item_templates_and_resolves_item_click_behavior()
    {
        var normalized = (CreateTimeline() with
        {
            Interaction = ZoorceTimelineInteraction.ReadOnly,
            SelectedItemId = "deployment",
            ItemTemplates = [new ZoorceTimelineItemTemplate(" deployment ", " /Pages/Shared/_Deployment.cshtml ", " deployment-template ")],
            Items =
            [
                new ZoorceTimelineItem("deployment", DateTimeOffset.UtcNow, "Deploy", TemplateKey: " deployment ", ClickBehavior: ZoorceTimelineItemClickBehavior.Command, CommandId: " inspect ", TargetUrl: " /deployments/1 "),
                new ZoorceTimelineItem("notice", DateTimeOffset.UtcNow.AddMinutes(1), "Notice", ClickBehavior: (ZoorceTimelineItemClickBehavior)999)
            ]
        }).Normalize();

        var deployment = normalized.Items.Single(item => item.Id == "deployment");
        var notice = normalized.Items.Single(item => item.Id == "notice");
        Assert.Equal("deployment", normalized.ItemTemplates.Single().Key);
        Assert.Equal("/Pages/Shared/_Deployment.cshtml", normalized.ItemTemplates.Single().PartialViewName);
        Assert.Equal("deployment-template", normalized.ItemTemplates.Single().ClientTemplateId);
        Assert.Equal("deployment", deployment.TemplateKey);
        Assert.Equal("inspect", deployment.CommandId);
        Assert.Equal("/deployments/1", deployment.TargetUrl);
        Assert.Equal(ZoorceTimelineItemClickBehavior.Command, normalized.ResolveClickBehavior(deployment));
        Assert.Equal(ZoorceTimelineItemClickBehavior.Inherit, notice.ClickBehavior);
        Assert.Equal(ZoorceTimelineItemClickBehavior.None, normalized.ResolveClickBehavior(notice));
    }

    [Fact]
    public void Timeline_rejects_unknown_or_duplicate_item_templates()
    {
        Assert.Throws<ArgumentException>(() => (CreateTimeline() with
        {
            Items = [new ZoorceTimelineItem("unknown", DateTimeOffset.UtcNow, "Unknown", TemplateKey: "missing")]
        }).Normalize());

        Assert.Throws<ArgumentException>(() => (CreateTimeline() with
        {
            ItemTemplates =
            [
                new ZoorceTimelineItemTemplate("card", "/Pages/Shared/_Card.cshtml"),
                new ZoorceTimelineItemTemplate(" CARD ", "/Pages/Shared/_AnotherCard.cshtml")
            ]
        }).Normalize());
    }

    [Fact]
    public void Timeline_rejects_invalid_identity_items_and_selection()
    {
        Assert.Throws<ArgumentException>(() => (CreateTimeline() with { Id = " " }).Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateTimeline() with { Items = null! }).Normalize());
        Assert.Throws<ArgumentNullException>(() => (CreateTimeline() with { Items = [null!] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateTimeline() with { Items = [new ZoorceTimelineItem(" ", DateTimeOffset.UtcNow, "One")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateTimeline() with { Items = [new ZoorceTimelineItem("same", DateTimeOffset.UtcNow, "One"), new ZoorceTimelineItem(" SAME ", DateTimeOffset.UtcNow, "Two")] }).Normalize());
        Assert.Throws<ArgumentException>(() => (CreateTimeline() with { SelectedItemId = "missing" }).Normalize());
    }

    [Fact]
    public void Timeline_contracts_and_view_component_are_available()
    {
        var actor = new ZoorceTimelineActor("Ada", "A", "/ada.png", "Owner");
        var action = new ZoorceTimelineAction("open", "Open", "↗", "/open");
        var item = new ZoorceTimelineItem("welcome", DateTimeOffset.UtcNow, "Welcome", Actor: actor, Action: action, IsRead: false, IsPinned: true, IsSelectable: false);
        var component = new ZoorceTimelineViewComponent();

        Assert.Equal("Ada", actor.Name);
        Assert.Equal("open", action.Id);
        Assert.False(item.IsRead);
        Assert.True(item.IsPinned);
        Assert.False(item.IsSelectable);
        Assert.Throws<ArgumentNullException>(() => component.Invoke(null!));
        var result = Assert.IsType<ViewViewComponentResult>(component.Invoke(CreateTimeline() with { Id = " timeline-view " }));
        var model = Assert.IsType<ZoorceTimelineOptions>(result.ViewData!.Model);
        Assert.Equal("timeline-view", model.Id);
        Assert.Contains(ZoorceTimelineVariant.Presentation, Enum.GetValues<ZoorceTimelineVariant>());
        Assert.DoesNotContain("Roadmap", Enum.GetNames<ZoorceTimelineVariant>());
        Assert.Null(typeof(ZoorceTimelinePresentation).GetProperty("UseArrowSegments"));
        Assert.Contains(ZoorceTimelineOrientation.Auto, Enum.GetValues<ZoorceTimelineOrientation>());
        Assert.Contains(ZoorceTimelineOverflowMode.Stack, Enum.GetValues<ZoorceTimelineOverflowMode>());
        Assert.Contains(ZoorceTimelineInteraction.Command, Enum.GetValues<ZoorceTimelineInteraction>());
        Assert.Contains(ZoorceTimelineItemClickBehavior.Custom, Enum.GetValues<ZoorceTimelineItemClickBehavior>());
        Assert.Contains(ZoorceTimelineGroupBy.Year, Enum.GetValues<ZoorceTimelineGroupBy>());
        Assert.Contains(ZoorceTimelineTone.Danger, Enum.GetValues<ZoorceTimelineTone>());
        Assert.Contains(ZoorceTimelineState.Error, Enum.GetValues<ZoorceTimelineState>());
        Assert.Contains(ZoorceTimelinePlacement.Below, Enum.GetValues<ZoorceTimelinePlacement>());
        Assert.Contains(ZoorceTimelineMarkerStyle.Ring, Enum.GetValues<ZoorceTimelineMarkerStyle>());
        Assert.Contains(ZoorceTimelineConnectorStyle.Curved, Enum.GetValues<ZoorceTimelineConnectorStyle>());
        Assert.Contains(ZoorceTimelineActiveMotion.Reveal, Enum.GetValues<ZoorceTimelineActiveMotion>());
        Assert.Contains(ZoorceTimelineRefreshMode.Replace, Enum.GetValues<ZoorceTimelineRefreshMode>());
        Assert.Contains(ZoorceTimelineRealtimeTransport.ServerSentEvents, Enum.GetValues<ZoorceTimelineRealtimeTransport>());
    }

    [Fact]
    public void Form_control_normalizes_reusable_configuration()
    {
        var options = new ZoorceFormControlOptions
        {
            Id = " release-owner ",
            Name = " owner ",
            Label = " Release owner ",
            Description = " Team responsible for delivery ",
            Kind = ZoorceFormControlKind.MultiSelect,
            Value = " platform, design ",
            Minimum = 0,
            Maximum = 10,
            Step = 1,
            Rows = 99,
            Locale = " es-MX ",
            CurrencyCode = " mxn ",
            Choices = [new(" platform ", " Platform "), new("design", " ")],
            Metadata = new Dictionary<string, string?> { [" mode "] = " compact ", ["ignored"] = " " }
        };

        var normalized = options.Normalize();

        Assert.Equal("release-owner", normalized.Id);
        Assert.Equal("owner", normalized.Name);
        Assert.Equal("Release owner", normalized.Label);
        Assert.Equal("platform, design", normalized.Value);
        Assert.Equal(18, normalized.Rows);
        Assert.Equal("es-MX", normalized.Locale);
        Assert.Equal("MXN", normalized.CurrencyCode);
        Assert.Equal("platform", normalized.Choices[0].Value);
        Assert.Equal("design", normalized.Choices[1].Label);
        Assert.Equal("compact", normalized.Metadata["mode"]);
        Assert.Null(normalized.Metadata["ignored"]);
        Assert.True(normalized.GetSelectedValues().SetEquals(["platform", "design"]));
    }

    [Fact]
    public void Form_control_rejects_invalid_bounds_identity_and_choices()
    {
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = " " }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "amount", Step = 0 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "code", Kind = ZoorceFormControlKind.Otp, CodeLength = 0 }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "amount", Minimum = 3, Maximum = 2 }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "choice", Choices = [new("same", "One"), new(" SAME ", "Two")] }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "locale", Locale = "not_a_locale" }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "currency", CurrencyCode = "US" }.Normalize());
    }

    [Fact]
    public void Otp_control_normalizes_length_character_set_and_case()
    {
        var code = new ZoorceFormControlOptions
        {
            Id = "release-code",
            Kind = ZoorceFormControlKind.Otp,
            Value = " a2-b9x ",
            CodeLength = 4,
            OtpCharacterSet = ZoorceOtpCharacterSet.Alphanumeric,
            OtpTextTransform = ZoorceOtpTextTransform.Uppercase
        }.Normalize();

        var numericCode = new ZoorceFormControlOptions
        {
            Id = "legacy-code",
            Kind = ZoorceFormControlKind.Otp,
            Value = "ab1237",
            MaximumLength = 4
        }.Normalize();

        Assert.Equal(4, code.CodeLength);
        Assert.Equal(4, code.GetOtpLength());
        Assert.Equal("A2B9", code.Value);
        Assert.Equal(4, numericCode.CodeLength);
        Assert.Equal("1237", numericCode.Value);
        Assert.Contains(ZoorceOtpCharacterSet.Alphanumeric, Enum.GetValues<ZoorceOtpCharacterSet>());
        Assert.Contains(ZoorceOtpTextTransform.Lowercase, Enum.GetValues<ZoorceOtpTextTransform>());
    }

    [Fact]
    public void File_upload_normalizes_limits_existing_assets_and_avatar_rules()
    {
        var gallery = new ZoorceFormControlOptions
        {
            Id = " release-gallery ",
            Kind = ZoorceFormControlKind.ImageUpload,
            Accept = " image/png, image/webp ",
            Multiple = true,
            MaximumFiles = 4,
            MaximumFileSizeBytes = 4_000_000,
            MaximumTotalFileSizeBytes = 8_000_000,
            UploadUrl = " /api/uploads ",
            FileUploadBehavior = ZoorceFileUploadBehavior.Manual,
            UploadButtonText = " Upload now ",
            ExistingFiles = [new(" cover ", " Cover.png ", " /assets/cover.png ", " IMAGE/PNG ", 512)]
        }.Normalize();

        var avatar = new ZoorceFormControlOptions
        {
            Id = "avatar",
            Kind = ZoorceFormControlKind.AvatarPicker,
            Multiple = true,
            ExistingFiles = [new("owner", "Owner.webp", "/assets/owner.webp", "image/webp", 300)]
        }.Normalize();

        Assert.Equal("release-gallery", gallery.Id);
        Assert.Equal("image/png, image/webp", gallery.Accept);
        Assert.True(gallery.Multiple);
        Assert.Equal(4, gallery.MaximumFiles);
        Assert.Equal("/api/uploads", gallery.UploadUrl);
        Assert.Equal("Upload now", gallery.UploadButtonText);
        Assert.Equal("cover", gallery.ExistingFiles.Single().Id);
        Assert.Equal("image/png", gallery.ExistingFiles.Single().ContentType);
        Assert.False(avatar.Multiple);
        Assert.Equal(1, avatar.MaximumFiles);
        Assert.Single(avatar.ExistingFiles);
    }

    [Fact]
    public void File_upload_rejects_invalid_limits_behaviors_and_avatar_collections()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "files", MaximumFiles = 0 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "files", MaximumFileSizeBytes = 0 }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "files", FileUploadBehavior = ZoorceFileUploadBehavior.Automatic }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions
        {
            Id = "avatar",
            Kind = ZoorceFormControlKind.AvatarPicker,
            ExistingFiles = [new("one", "one.png"), new("two", "two.png")]
        }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions
        {
            Id = "files",
            ExistingFiles = [new("same", "one.png"), new(" SAME ", "two.png")]
        }.Normalize());
    }

    [Fact]
    public void Form_controls_and_chips_are_available_as_isolated_view_components()
    {
        var control = new ZoorceFormControlViewComponent();
        var chip = new ZoorceChipViewComponent();

        Assert.Throws<ArgumentNullException>(() => control.Invoke(null!));
        Assert.Throws<ArgumentNullException>(() => chip.Invoke(null!));

        var controlResult = Assert.IsType<ViewViewComponentResult>(control.Invoke(new ZoorceFormControlOptions { Id = "quantity", Kind = ZoorceFormControlKind.QuantityStepper, Value = "3" }));
        var controlModel = Assert.IsType<ZoorceFormControlOptions>(controlResult.ViewData!.Model);
        var chipResult = Assert.IsType<ViewViewComponentResult>(chip.Invoke(new ZoorceChipOptions { Text = " Stable ", Tone = " SUCCESS " }));
        var chipModel = Assert.IsType<ZoorceChipOptions>(chipResult.ViewData!.Model);

        Assert.Equal("quantity", controlModel.Id);
        Assert.Equal("Stable", chipModel.Text);
        Assert.Equal("success", chipModel.Tone);
        Assert.Contains(ZoorceFormControlKind.ColorPicker, Enum.GetValues<ZoorceFormControlKind>());
        Assert.Contains(ZoorceFormControlKind.ToggleSwitch, Enum.GetValues<ZoorceFormControlKind>());
        Assert.Contains(ZoorceFormControlKind.Otp, Enum.GetValues<ZoorceFormControlKind>());
        Assert.DoesNotContain("Json", Enum.GetNames<ZoorceFormControlKind>());
        Assert.Contains(ZoorceFormControlState.Error, Enum.GetValues<ZoorceFormControlState>());
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

    private static ZoorceStepperOptions CreateStepper() => new()
    {
        Id = "release-stepper",
        AriaLabel = "Release workflow",
        CurrentStepId = "brief",
        PreviousLabel = "Previous",
        NextLabel = "Continue now",
        CompleteLabel = "Finish",
        Steps =
        [
            new ZoorceStepperStep("brief", "Brief", State: ZoorceStepperState.Active),
            new ZoorceStepperStep("validate", "Validate")
        ]
    };

    private static ZoorceTimelineOptions CreateTimeline() => new()
    {
        Id = "operations",
        AriaLabel = "Operations timeline",
        SelectedItemId = "first",
        Items =
        [
            new ZoorceTimelineItem("first", DateTimeOffset.UtcNow, "First"),
            new ZoorceTimelineItem("second", DateTimeOffset.UtcNow.AddHours(1), "Second")
        ]
    };
}
