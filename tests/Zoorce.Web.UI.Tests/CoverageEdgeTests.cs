using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zoorce.Web.UI.Abstractions.Data;
using Zoorce.Web.UI.Charts;
using Zoorce.Web.UI.Data;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Foundation.Progress;
using Zoorce.Web.UI.Foundation.WaitDialog;
using Zoorce.Web.UI.Layout.Components;
using Zoorce.Web.UI.Markdown;
using Zoorce.Web.UI.Theming;
using Zoorce.Web.UI.Timelines;

namespace Zoorce.Web.UI.Tests;

/// <summary>Exercises defensive branches that protect component contracts from incomplete host input.</summary>
public sealed class CoverageEdgeTests
{
    [Fact]
    public void Timeline_validates_templates_and_resolves_each_interaction_behavior()
    {
        var item = new ZoorceTimelineItem("item", DateTimeOffset.UtcNow, "Item", TemplateKey: "unknown");
        Assert.Throws<ArgumentException>(() => new ZoorceTimelineOptions { Id = "timeline", Items = [item] }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceTimelineOptions { Id = "timeline", ItemTemplates = [new(" ", "partial")] }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceTimelineOptions { Id = "timeline", ItemTemplates = [new("card", " ")] }.Normalize());

        var baseItem = new ZoorceTimelineItem("item", DateTimeOffset.UtcNow, "Item");
        Assert.Equal(ZoorceTimelineItemClickBehavior.Select, new ZoorceTimelineOptions { Interaction = ZoorceTimelineInteraction.Select }.ResolveClickBehavior(baseItem));
        Assert.Equal(ZoorceTimelineItemClickBehavior.Navigate, new ZoorceTimelineOptions { Interaction = ZoorceTimelineInteraction.Navigate }.ResolveClickBehavior(baseItem));
        Assert.Equal(ZoorceTimelineItemClickBehavior.Select, new ZoorceTimelineOptions { Interaction = ZoorceTimelineInteraction.Command }.ResolveClickBehavior(baseItem));
        Assert.Equal(ZoorceTimelineItemClickBehavior.Command, new ZoorceTimelineOptions().ResolveClickBehavior(baseItem with { ClickBehavior = ZoorceTimelineItemClickBehavior.Command }));
    }

    [Fact]
    public void Foundation_options_preserve_boolean_configuration_and_top_bar_model_keeps_host_data()
    {
        var progress = new ZoorceProgressOptions { ShowValue = true }.Normalize();
        var wait = new ZoorceWaitDialogOptions { Id = "wait", CanCancel = true }.Normalize();
        var button = new ZoorceButtonOptions { Text = "Edit", Disabled = true, Compact = true }.Normalize();
        Assert.True(progress.ShowValue);
        Assert.True(wait.CanCancel);
        Assert.True(button.Disabled);
        Assert.True(button.Compact);

        var profile = new Zoorce.Web.UI.Abstractions.UserContext.UserMenuProfile("Ada");
        var topBar = new ZoorceTopBarViewComponent(Options.Create(new Zoorce.Web.UI.Layout.ZoorceShellOptions { ApplicationName = "Host" }), new TestUserProvider(profile));
        var model = Assert.IsType<ZoorceTopBarModel>(Assert.IsType<Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult>(topBar.Invoke()).ViewData!.Model);
        Assert.Equal("Host", model.ApplicationName);
        Assert.Same(profile, model.Profile);
    }

    [Fact]
    public async Task Data_controller_discards_a_late_result_and_bus_validates_subscription_input()
    {
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var source = new DelegateSource<int>(async (_, _, _) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task;
                return new ZoorceDataPage<int>([1]);
            }

            return new ZoorceDataPage<int>([2]);
        });
        await using var controller = new ZoorceDataController<int>(source);
        var first = controller.LoadAsync();
        await firstStarted.Task;
        var second = await controller.LoadAsync();
        releaseFirst.TrySetResult();
        await first;
        Assert.Equal([2], second.Page!.Items);
        Assert.Equal([2], controller.Snapshot.Page!.Items);

        var bus = new ZoorceDataInvalidationBus();
        Assert.Throws<ArgumentException>(() => bus.Subscribe(" ", (_, _) => ValueTask.CompletedTask));
        Assert.Throws<ArgumentNullException>(() => bus.Subscribe("orders", null!));
        await bus.PublishAsync(new ZoorceDataInvalidation { Scope = "without-listeners" });
    }

    [Fact]
    public void Aggregate_registration_configures_the_runtime_theme_and_markdown_skips_empty_headings()
    {
        var services = new ServiceCollection();
        Zoorce.Web.UI.ServiceCollectionExtensions.AddZoorceWebUI(services, shell =>
        {
            shell.ApplicationName = "Host";
            shell.Themes.UsePreset(ZoorceThemePreset.Neutral);
        });
        using var provider = services.BuildServiceProvider();
        Assert.Equal("dark", provider.GetRequiredService<IZoorceThemeService>().Current.Name);

        var markdown = provider.GetRequiredService<IZoorceMarkdownRenderer>().Render("# Visible\n\n#\n\n[bad](javascript:alert(1))");
        Assert.Single(markdown.TableOfContents);
        Assert.Contains("href=\"#\"", markdown.Html.ToString());

        var chart = new ZoorceChartOptions { Id = "chart", Series = [new ZoorceChartSeries("one", [new ZoorceChartPoint("now", 1)])], ValueSuffix = " " }.Normalize();
        Assert.Null(chart.ValueSuffix);
    }

    private sealed class TestUserProvider(Zoorce.Web.UI.Abstractions.UserContext.UserMenuProfile profile) : Zoorce.Web.UI.Abstractions.UserContext.IUserMenuProvider
    {
        public Zoorce.Web.UI.Abstractions.UserContext.UserMenuProfile GetCurrentUser() => profile;
    }

    private sealed class DelegateSource<T>(Func<ZoorceDataQuery, ZoorcePageRequest, CancellationToken, Task<ZoorceDataPage<T>>> load) : IZoorceDataSource<T>
    {
        public Task<ZoorceDataPage<T>> QueryAsync(ZoorceDataQuery query, ZoorcePageRequest pageRequest, CancellationToken cancellationToken = default)
            => load(query, pageRequest, cancellationToken);
    }
}
