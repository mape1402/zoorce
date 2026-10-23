namespace Zoorce.Web.UI.Theming;

/// <summary>Creates complete, accessible palette pairs for common product identities.</summary>
public static class ZoorceThemePresets
{
    /// <summary>Applies a complete preset to both configured color modes.</summary>
    public static void Apply(ZoorceThemeCatalogOptions options, ZoorceThemePreset preset)
    {
        ArgumentNullException.ThrowIfNull(options);

        var palettes = preset switch
        {
            ZoorceThemePreset.Verdant => (ZoorceThemePalette.CreateLight(), ZoorceThemePalette.CreateDark()),
            ZoorceThemePreset.Amethyst => (CreateAmethystLight(), CreateAmethystDark()),
            ZoorceThemePreset.Neutral => (CreateNeutralLight(), CreateNeutralDark()),
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "The theme preset is not supported.")
        };

        options.Light.CopyFrom(palettes.Item1);
        options.Dark.CopyFrom(palettes.Item2);
    }

    private static ZoorceThemePalette CreateAmethystLight() => new()
    {
        Canvas = "#faf8ff", Surface = "#ffffff", Elevated = "#ffffff", Subtle = "#f1edff",
        Text = "#211b2e", Muted = "#665d78", Border = "#ddd6ee", Primary = "#6d28d9",
        PrimaryForeground = "#ffffff", PrimaryHover = "#5b21b6", PrimarySoft = "#ede9fe",
        Success = "#087a57", SuccessSoft = "#ddf4e9", Warning = "#8a5a00", WarningSoft = "#fff1d6",
        Danger = "#b4233f", DangerForeground = "#ffffff", DangerSoft = "#ffe7ec", Info = "#285eb8",
        InfoSoft = "#e6eeff", DataPrimary = "#2563c9", DataSecondary = "#7c3aed", DataTertiary = "#c026d3",
        DataQuaternary = "#d97706", DataQuinary = "#0f766e", Sidebar = "#f3efff", SidebarText = "#2d2145",
        SidebarMuted = "#74668c", Glow = "rgba(109, 40, 217, 0.10)", Shadow = "rgba(46, 30, 80, 0.14)",
        CodeCanvas = "#171022", CodeText = "#ece5ff", CodeBorder = "#443461", CodeControlSurface = "#261a37",
        CodeControlText = "#c9b8ff", Overlay = "rgba(26, 16, 43, 0.56)"
    };

    private static ZoorceThemePalette CreateAmethystDark() => new()
    {
        Canvas = "#0d0a13", Surface = "#16101f", Elevated = "#1d1529", Subtle = "#281d37",
        Text = "#f7f2ff", Muted = "#b9afc9", Border = "#39294d", Primary = "#a78bfa",
        PrimaryForeground = "#160b2c", PrimaryHover = "#c4b5fd", PrimarySoft = "#332252",
        Success = "#39d98a", SuccessSoft = "#123d2b", Warning = "#f2bd58", WarningSoft = "#3a2c12",
        Danger = "#ff8291", DangerForeground = "#29070d", DangerSoft = "#491720", Info = "#94b6ff",
        InfoSoft = "#1c2b52", DataPrimary = "#7fb0ff", DataSecondary = "#b494ff", DataTertiary = "#f08dde",
        DataQuaternary = "#ffc86a", DataQuinary = "#56d5c7", Sidebar = "#120d1b", SidebarText = "#f7f2ff",
        SidebarMuted = "#aca0c3", Glow = "rgba(167, 139, 250, 0.24)", Shadow = "rgba(0, 0, 0, 0.48)",
        CodeCanvas = "#110b1a", CodeText = "#e9ddff", CodeBorder = "#4b3966", CodeControlSurface = "#24172f",
        CodeControlText = "#d0bdff", Overlay = "rgba(7, 4, 11, 0.62)"
    };

    private static ZoorceThemePalette CreateNeutralLight() => new()
    {
        Canvas = "#f7f8fb", Surface = "#ffffff", Elevated = "#ffffff", Subtle = "#eef1f6",
        Text = "#18202f", Muted = "#5d687a", Border = "#d8dee9", Primary = "#4056c8",
        PrimaryForeground = "#ffffff", PrimaryHover = "#3045ad", PrimarySoft = "#e5e9ff",
        Success = "#087a57", SuccessSoft = "#ddf4e9", Warning = "#8a5a00", WarningSoft = "#fff1d6",
        Danger = "#b4233f", DangerForeground = "#ffffff", DangerSoft = "#ffe7ec", Info = "#285eb8",
        InfoSoft = "#e6eeff", DataPrimary = "#2563c9", DataSecondary = "#7253c6", DataTertiary = "#b9369d",
        DataQuaternary = "#c57608", DataQuinary = "#087d75", Sidebar = "#f1f4f9", SidebarText = "#1d2b42",
        SidebarMuted = "#62708a", Glow = "rgba(64, 86, 200, 0.09)", Shadow = "rgba(24, 32, 47, 0.13)",
        CodeCanvas = "#121a29", CodeText = "#dce6ff", CodeBorder = "#34445f", CodeControlSurface = "#1b2940",
        CodeControlText = "#aec7ff", Overlay = "rgba(16, 23, 35, 0.54)"
    };

    private static ZoorceThemePalette CreateNeutralDark() => new()
    {
        Canvas = "#0c111a", Surface = "#121a26", Elevated = "#182235", Subtle = "#202d43",
        Text = "#eef4ff", Muted = "#aab6c9", Border = "#2c3b55", Primary = "#91a7ff",
        PrimaryForeground = "#111936", PrimaryHover = "#bac7ff", PrimarySoft = "#26386c",
        Success = "#39d98a", SuccessSoft = "#123d2b", Warning = "#f2bd58", WarningSoft = "#3a2c12",
        Danger = "#ff8291", DangerForeground = "#29070d", DangerSoft = "#491720", Info = "#9fc0ff",
        InfoSoft = "#1d3159", DataPrimary = "#7fb0ff", DataSecondary = "#b89dff", DataTertiary = "#ed91d5",
        DataQuaternary = "#ffd06c", DataQuinary = "#5dd3c4", Sidebar = "#101824", SidebarText = "#eef4ff",
        SidebarMuted = "#9daec8", Glow = "rgba(145, 167, 255, 0.22)", Shadow = "rgba(0, 0, 0, 0.48)",
        CodeCanvas = "#0a101a", CodeText = "#dbe7ff", CodeBorder = "#32415a", CodeControlSurface = "#152237",
        CodeControlText = "#b8ceff", Overlay = "rgba(4, 8, 14, 0.62)"
    };
}
