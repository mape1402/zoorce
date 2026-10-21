using Zoerce.WebUI;
using Zoerce.WebUI.Abstractions.Navigation;
using Zoerce.WebUI.Abstractions.UserContext;
using Zoerce.WebUI.Options;
using Zoerce.WebUI.Sample.Infrastructure;
using Zoerce.WebUI.Abstractions.Data;
using Zoerce.WebUI.Components.Stepper;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddZoerceWebUI(options =>
{
    options.ApplicationName = "Zoorce";
    options.BrandMark = "Z";
    options.DefaultTheme = ZoorceThemeMode.Dark;
    options.UseThemePreset(ZoorceThemePreset.Amethyst);
    options.SidebarMode = ZoorceSidebarMode.Collapsible;
    options.SidebarCollapsedByDefault = false;
    options.PersistSidebarState = true;
    options.NavigationDensity = ZoorceNavigationDensity.Compact;
});
builder.Services.AddScoped<INavigationProvider, SampleNavigationProvider>();
builder.Services.AddScoped<IUserMenuProvider, SampleUserMenuProvider>();
builder.Services.AddSingleton<SampleComponentCatalog>();
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
app.MapGet("/api/steppers/release", () => Results.Ok(SampleStepperData.Create()));
app.MapPost("/api/steppers/policy-transition", (ZoorceStepperTransitionRequest request) => Results.Ok(SampleStepperData.Authorize(request)));
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

app.Run();
