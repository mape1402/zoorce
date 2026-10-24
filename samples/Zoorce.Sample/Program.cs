using Zoorce.Web.UI;
using Zoorce.Web.UI.Abstractions.Navigation;
using Zoorce.Web.UI.Abstractions.UserContext;
using Zoorce.Web.UI.Layout;
using Zoorce.Web.UI.Theming;
using Zoorce.Web.UI.Realtime.SignalR;
using Zoorce.Sample.Infrastructure;
using Zoorce.Web.UI.Abstractions.Data;
using Zoorce.Web.UI.Navigation.Stepper;
using Zoorce.Web.UI.Navigation.Wizard;
using Zoorce.Web.UI.Timelines;
using Zoorce.Web.UI.Diagrams;
using Zoorce.Web.UI.Foundation.Components;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddZoorceWebUI(options =>
{
    options.ApplicationName = "Zoorce";
    options.BrandMark = "Z";
    options.DefaultTheme = ZoorceThemeMode.Dark;
    options.UseThemePreset(ZoorceThemePreset.Amethyst);
    options.SidebarMode = ZoorceSidebarMode.Collapsible;
    options.SidebarCollapsedByDefault = false;
    options.PersistSidebarState = true;
    options.NavigationExpansion = ZoorceNavigationExpansionMode.AllCollapsed;
    options.PersistNavigationExpansion = true;
    options.NavigationDensity = ZoorceNavigationDensity.Compact;
    options.NotificationCenter = new ZoorceNotificationCenterOptions
    {
        Id = "shell-notifications",
        Label = "Notifications",
        Items =
        [
            new ZoorceNotificationItem { Id = "approval", Title = "Approval completed", Description = "Design approved the release proposal.", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-12), Tone = ZoorceNotificationTone.Success, ActionText = "Review" },
            new ZoorceNotificationItem { Id = "capacity", Title = "Capacity threshold reached", Description = "One production pool needs an owner.", Timestamp = DateTimeOffset.UtcNow.AddHours(-1), Tone = ZoorceNotificationTone.Warning, ActionText = "Inspect" },
            new ZoorceNotificationItem { Id = "digest", Title = "Daily digest is ready", Description = "A summary of workspace activity is available.", Timestamp = DateTimeOffset.UtcNow.AddDays(-1), IsRead = true }
        ]
    };
});
builder.Services.AddScoped<INavigationProvider, SampleNavigationProvider>();
builder.Services.AddScoped<IUserMenuProvider, SampleUserMenuProvider>();
builder.Services.AddZoorceSignalRRealtime();
builder.Services.AddSingleton<SampleComponentCatalog>();
builder.Services.AddSingleton<SampleDiagramCatalog>();
builder.Services.AddSingleton<SampleReleaseDraftStore>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<SampleRealtimePublisher>();
builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var signalrClientDirectory = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "node_modules", "@microsoft", "signalr", "dist", "browser"));
if (Directory.Exists(signalrClientDirectory))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(signalrClientDirectory),
        RequestPath = "/vendor/signalr"
    });
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ComponentActivityHub>("/hubs/component-activity");
app.MapGet("/api/components", async (string? search, string? sort, bool? descending, int? page, int? pageSize, SampleComponentCatalog catalog, CancellationToken cancellationToken) =>
{
    var query = new ZoorceDataQuery(search, sort, descending ?? false);
    var request = new ZoorcePageRequest { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await catalog.QueryAsync(query, request, cancellationToken));
});
app.MapGet("/api/charts/adoption", () => Results.Ok(SampleChartData.Create()));
app.MapGet("/api/diagrams/{key}", (string key, SampleDiagramCatalog catalog) => Results.Ok(catalog.Get(key)));
app.MapPut("/api/diagrams/{key}", (string key, ZoorceDiagramDocument document, SampleDiagramCatalog catalog) => Results.Ok(catalog.Save(key, document)));
app.MapGet("/api/steppers/release", () => Results.Ok(SampleStepperData.Create()));
app.MapPost("/api/steppers/policy-transition", (ZoorceStepperTransitionRequest request) => Results.Ok(SampleStepperData.Authorize(request)));
app.MapPost("/api/wizards/release-transition", (ZoorceWizardTransitionRequest request) =>
{
    var values = request.Values ?? new Dictionary<string, string[]>();
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    string Value(string key) => values.TryGetValue(key, out var captured) ? captured.FirstOrDefault() ?? string.Empty : string.Empty;

    var isForwardStepChange = !request.IsBackward && request.TargetStepId is not null;
    if (isForwardStepChange && request.CurrentStepId == "scope" && string.IsNullOrWhiteSpace(Value("releaseName")))
    {
        errors["releaseName"] = ["Name the release before continuing."];
    }
    if (isForwardStepChange && request.CurrentStepId == "readiness" && Value("approval") != "approved")
    {
        errors["approval"] = ["Explicit release-owner approval is required."];
    }
    if (errors.Count > 0)
    {
        return Results.Ok(new ZoorceWizardTransitionDecision(false, "Resolve the highlighted fields before continuing.", FieldErrors: errors));
    }

    if (request.Action == "save")
    {
        return Results.Ok(new ZoorceWizardTransitionDecision(true, "Draft saved by the sample host."));
    }
    if (request.Action == "cancel")
    {
        return Results.Ok(new ZoorceWizardTransitionDecision(true, "Cancellation request acknowledged. The host decides where to navigate next."));
    }
    if (request.Action == "finish")
    {
        return Results.Ok(new ZoorceWizardTransitionDecision(true, "Release workflow finished.", new ZoorceWizardSnapshot("confirm",
        [
            new ZoorceWizardStepSnapshot("scope", ZoorceStepperState.Completed),
            new ZoorceWizardStepSnapshot("readiness", ZoorceStepperState.Completed),
            new ZoorceWizardStepSnapshot("confirm", ZoorceStepperState.Completed)
        ])));
    }

    var target = request.TargetStepId ?? request.CurrentStepId;
    var ordered = new[] { "scope", "readiness", "confirm" };
    var targetIndex = Array.IndexOf(ordered, target);
    var snapshot = new ZoorceWizardSnapshot(target,
        ordered.Select((step, index) => new ZoorceWizardStepSnapshot(step,
            index < targetIndex ? ZoorceStepperState.Completed : index == targetIndex ? ZoorceStepperState.Active : ZoorceStepperState.Pending)).ToArray());
    return Results.Ok(new ZoorceWizardTransitionDecision(true, "Transition authorized by the sample host.", snapshot));
});
app.MapGet("/api/timelines/operations", () => Results.Ok(SampleTimelineData.CreateOperationsSnapshot()));
app.MapPost("/api/demo/activity", async (SampleRealtimePublisher publisher, CancellationToken cancellationToken) =>
{
    var changed = await publisher.PublishAsync(cancellationToken);
    return Results.Ok(new { changed.Id, changed.Name });
});
app.MapPost("/api/demo/stepper", async (SampleRealtimePublisher publisher, CancellationToken cancellationToken) =>
{
    var snapshot = await publisher.PublishStepperAsync(cancellationToken);
    return Results.Ok(snapshot);
});
app.MapPost("/api/demo/timeline", async (SampleRealtimePublisher publisher, CancellationToken cancellationToken) =>
{
    var update = await publisher.PublishTimelineAsync(cancellationToken);
    return Results.Ok(update);
});
app.MapPost("/api/demo/uploads", async (HttpRequest request, CancellationToken cancellationToken) =>
{
    var form = await request.ReadFormAsync(cancellationToken);
    var files = form.Files;
    await Task.Delay(180, cancellationToken);
    return Results.Ok(new
    {
        uploaded = files.Select(file => new { file.FileName, file.ContentType, file.Length }).ToArray(),
        message = $"{files.Count} file(s) accepted by the sample endpoint."
    });
});
app.MapPost("/api/demo/release-drafts", async (HttpRequest request, SampleReleaseDraftStore drafts, CancellationToken cancellationToken) =>
{
    var form = await request.ReadFormAsync(cancellationToken);
    var releaseName = form["releaseName"].ToString().Trim();
    var risk = form["risk"].ToString().Trim();
    var owner = form["owner"].ToString().Trim();
    var confidenceText = form["confidence"].ToString();
    var confidence = int.TryParse(confidenceText, out var parsedConfidence) ? parsedConfidence : 0;
    var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    if (string.IsNullOrWhiteSpace(releaseName)) errors["releaseName"] = ["A release name is required."];
    if (string.IsNullOrWhiteSpace(risk)) errors["risk"] = ["Choose the delivery risk."];
    if (string.IsNullOrWhiteSpace(owner)) errors["owner"] = ["Assign a release owner."];
    if (confidence is < 1 or > 5) errors["confidence"] = ["Choose a launch confidence between one and five."];
    if (string.Equals(risk, "high", StringComparison.OrdinalIgnoreCase) && confidence < 4)
    {
        errors["confidence"] = ["High-risk releases require at least four stars of launch confidence."];
    }

    if (errors.Count > 0) return Results.ValidationProblem(errors);

    var checks = form["checks"].Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToArray();
    return Results.Ok(drafts.Save(releaseName, risk, owner, confidence, checks));
});

app.Run();
