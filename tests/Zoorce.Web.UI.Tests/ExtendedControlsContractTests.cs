using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoorce.Web.UI.Forms;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Layout;
using Zoorce.Web.UI.Navigation.Components;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies contracts for the drawer, notification, calendar, navigation and split-pane controls.</summary>
public sealed class ExtendedControlsContractTests
{
    [Fact]
    public void Drawer_and_notification_center_normalize_component_owned_state()
    {
        var triggerIcon = new HtmlString("<svg aria-hidden=\"true\"></svg>");
        Func<ZoorceNotificationItem, IHtmlContent> itemTemplate = item => new HtmlString($"<strong>{item.Title}</strong>");
        var drawer = new ZoorceDrawerViewComponent().Invoke(new ZoorceDrawerOptions
        {
            Id = " inspector ",
            Title = " Inspector ",
            Description = " ",
            Position = (ZoorceDrawerPosition)99,
            Size = (ZoorceDrawerSize)99,
            CssClass = " surface  elevated "
        });
        var notification = new ZoorceNotificationCenterViewComponent().Invoke(new ZoorceNotificationCenterOptions
        {
            Id = " alerts ",
            TriggerIcon = triggerIcon,
            ItemTemplate = itemTemplate,
            Items = [new ZoorceNotificationItem { Id = " release ", Title = " Release ready ", Tone = ZoorceNotificationTone.Success }]
        });

        var drawerModel = Assert.IsType<ZoorceDrawerOptions>(Assert.IsType<ViewViewComponentResult>(drawer).ViewData!.Model);
        var notificationModel = Assert.IsType<ZoorceNotificationCenterOptions>(Assert.IsType<ViewViewComponentResult>(notification).ViewData!.Model);
        Assert.Equal("inspector", drawerModel.Id);
        Assert.Equal(ZoorceDrawerPosition.End, drawerModel.Position);
        Assert.Equal(ZoorceDrawerSize.Default, drawerModel.Size);
        Assert.Equal("surface elevated", drawerModel.CssClass);
        Assert.Equal("alerts", notificationModel.Id);
        Assert.Equal("release", notificationModel.Items.Single().Id);
        Assert.Same(triggerIcon, notificationModel.TriggerIcon);
        Assert.Same(itemTemplate, notificationModel.ItemTemplate);
        Assert.Throws<ArgumentException>(() => new ZoorceNotificationCenterOptions
        {
            Id = "alerts",
            Items = [new ZoorceNotificationItem { Id = "same", Title = "One" }, new ZoorceNotificationItem { Id = "same", Title = "Two" }]
        }.Normalize());
    }

    [Fact]
    public void Date_picker_normalizes_range_and_rejects_invalid_calendar_contracts()
    {
        var result = new ZoorceDatePickerViewComponent().Invoke(new ZoorceDatePickerOptions
        {
            Id = " campaign ",
            Name = " campaign ",
            Mode = ZoorceDatePickerMode.Range,
            RangeStart = new DateOnly(2026, 10, 2),
            RangeEnd = new DateOnly(2026, 10, 4),
            DisabledDates = [new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 8)],
            Locale = " en-US "
        });
        var model = Assert.IsType<ZoorceDatePickerOptions>(Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);
        Assert.Equal("campaign", model.Id);
        Assert.Equal("campaign", model.Name);
        Assert.Single(model.DisabledDates);
        Assert.Equal("en-US", model.Locale);
        Assert.Throws<ArgumentException>(() => new ZoorceDatePickerOptions
        {
            Id = "campaign",
            Name = "campaign",
            Minimum = new DateOnly(2026, 10, 3),
            Maximum = new DateOnly(2026, 10, 2)
        }.Normalize());
    }

    [Fact]
    public void Time_picker_normalizes_the_clock_contract_and_rejects_invalid_bounds()
    {
        var result = new ZoorceTimePickerViewComponent().Invoke(new ZoorceTimePickerOptions
        {
            Id = " delivery-time ",
            Name = " deliveryTime ",
            Value = new TimeOnly(17, 42),
            Minimum = new TimeOnly(8, 0),
            Maximum = new TimeOnly(20, 0),
            MinuteStep = 15,
            Format = ZoorceTimePickerFormat.TwelveHour,
            Locale = " en-US "
        });
        var model = Assert.IsType<ZoorceTimePickerOptions>(Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);
        Assert.Equal("delivery-time", model.Id);
        Assert.Equal("deliveryTime", model.Name);
        Assert.Equal(15, model.MinuteStep);
        Assert.Equal(new TimeOnly(17, 42), model.Value);
        Assert.Equal("en-US", model.Locale);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceTimePickerOptions
        {
            Id = "delivery-time",
            Name = "deliveryTime",
            MinuteStep = 7
        }.Normalize());
    }

    [Fact]
    public void Breadcrumb_and_pagination_normalize_bounded_navigation()
    {
        var breadcrumb = new ZoorceBreadcrumbViewComponent().Invoke(new ZoorceBreadcrumbOptions
        {
            Id = " trail ",
            MaxVisible = 1,
            Items = [new ZoorceBreadcrumbItem { Text = " Workspace ", Href = " / " }, new ZoorceBreadcrumbItem { Text = " Current " }]
        });
        var pagination = new ZoorcePaginationViewComponent().Invoke(new ZoorcePaginationOptions
        {
            Id = " pages ",
            CurrentPage = 99,
            TotalPages = 4,
            PageSize = 17,
            PageSizes = [0, 10, 10],
            WindowSize = 99
        });
        var breadcrumbModel = Assert.IsType<ZoorceBreadcrumbOptions>(Assert.IsType<ViewViewComponentResult>(breadcrumb).ViewData!.Model);
        var paginationModel = Assert.IsType<ZoorcePaginationOptions>(Assert.IsType<ViewViewComponentResult>(pagination).ViewData!.Model);
        Assert.Equal(2, breadcrumbModel.MaxVisible);
        Assert.True(breadcrumbModel.Items.Last().Current);
        Assert.Equal(4, paginationModel.CurrentPage);
        Assert.Contains(17, paginationModel.PageSizes);
        Assert.Equal(11, paginationModel.WindowSize);
    }

    [Fact]
    public void Split_pane_normalizes_resizing_contract_and_rejects_incompatible_minimums()
    {
        var result = new ZoorceSplitPaneViewComponent().Invoke(new ZoorceSplitPaneOptions
        {
            Id = " inspector ",
            Primary = new HtmlString("primary"),
            Secondary = new HtmlString("secondary"),
            InitialPrimaryPercent = 5,
            MinimumPrimaryPercent = 25,
            MinimumSecondaryPercent = 30,
            PersistenceKey = " inspector-width "
        });
        var model = Assert.IsType<ZoorceSplitPaneOptions>(Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);
        Assert.Equal("inspector", model.Id);
        Assert.Equal(25, model.InitialPrimaryPercent);
        Assert.Equal("inspector-width", model.PersistenceKey);
        Assert.Throws<ArgumentException>(() => new ZoorceSplitPaneOptions
        {
            Id = "inspector",
            Primary = new HtmlString("primary"),
            Secondary = new HtmlString("secondary"),
            MinimumPrimaryPercent = 70,
            MinimumSecondaryPercent = 40
        }.Normalize());
    }
}
