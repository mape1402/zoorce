using Zoerce.WebUI;
using Zoerce.WebUI.Abstractions.Navigation;
using Zoerce.WebUI.Abstractions.UserContext;
using Zoerce.WebUI.Options;
using Zoerce.WebUI.Sample.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddZoerceWebUI(options =>
{
    options.ApplicationName = "Zoorce";
    options.BrandMark = "Z";
    options.DefaultTheme = ZoorceThemeMode.Dark;
    options.Dark.Primary = "#20e0a0";
    options.Dark.PrimaryHover = "#65f1c1";
    options.Dark.Glow = "rgba(32, 224, 160, 0.20)";
    options.Light.Primary = "#087a57";
    options.Light.PrimaryHover = "#056446";
    options.Light.Glow = "rgba(8, 122, 87, 0.08)";
});
builder.Services.AddScoped<INavigationProvider, SampleNavigationProvider>();
builder.Services.AddScoped<IUserMenuProvider, SampleUserMenuProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
