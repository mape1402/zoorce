# Testing

## Contract and integration suite

Run the complete suite with:

```powershell
dotnet test tests/Zoorce.Web.UI.Tests/Zoorce.Web.UI.Tests.csproj --no-restore
```

To exercise the production asset pipeline, publish the sample and run the generated host rather than using `dotnet run` with a forced Production environment. Static web assets from Razor class libraries are copied into the publish output, which is the deployment topology consumers use:

```powershell
dotnet publish samples/Zoorce.Sample/Zoorce.Sample.csproj --configuration Release
```

`npm run test:coverage` enforces at least 90% total line coverage for authored C# in the current `Zoorce.Web.UI.*` packages. The measure deliberately excludes Razor-generated methods, the test assembly and the superseded `Zoerce.WebUI` output; browser scenarios exercise rendered Razor behavior instead.

The suite includes a real local SignalR hub test. It proves that `Zoorce.Web.UI.Realtime.SignalR` forwards a hub event into the neutral invalidation bus without making Grid, Charts, Timelines, Steppers, or Diagrams depend on SignalR.

## Browser acceptance checks

Launch `Zoorce.Sample`, then verify at desktop and narrow widths:

1. Form controls retain keyboard and pointer behavior; numeric values, chips, OTP, files, avatar replacement, and gallery deletion update their own field only.
2. Data grid and chart can refresh independently through their own data-root and invalidation scope.
3. Timeline overflow remains inside its viewport; selection and custom-item actions remain usable.
4. Diagram focus, zoom, minimap, connection, and export surfaces do not alter other diagram instances.
5. Theme changes update semantic tokens without reloading or rebuilding the host page.
6. The console contains no errors, including when SignalR is not opted in by the host.
