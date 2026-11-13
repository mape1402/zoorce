namespace Zoorce.Web.UI.Theming;

/// <summary>Creates complete, accessible palette pairs for Zoorce product identities.</summary>
public static class ZoorceThemePresets
{
    private static readonly IReadOnlyList<PresetDefinition> Definitions =
    [
        new(new(ZoorceThemePreset.Verdant, "Verdant", "The original green-accented product palette.", "#087a57", "#20e7a0"), ZoorceThemePalette.CreateLight, ZoorceThemePalette.CreateDark),
        new(new(ZoorceThemePreset.Amethyst, "Amethyst", "Purple surfaces with a confident violet accent.", "#6d28d9", "#a78bfa"), CreateAmethystLight, CreateAmethystDark),
        new(new(ZoorceThemePreset.Neutral, "Neutral", "A blue-neutral palette with restrained chroma.", "#4056c8", "#91a7ff"), CreateNeutralLight, CreateNeutralDark),
        Branded(ZoorceThemePreset.Crimson, "Crimson", "A confident red palette.", ThemeFamily.Rose, "#be123c", "#ffffff", "#9f1239", "#ffe4e6", "#e11d48", "#fda4af", "#3b0713", "#fecdd3", "#4c1022", "#fb7185"),
        Branded(ZoorceThemePreset.Chestnut, "Chestnut", "A warm brown palette.", ThemeFamily.Warm, "#7c3f1d", "#ffffff", "#663416", "#fbe9dd", "#b45309", "#eaa373", "#2d1308", "#fac59f", "#452417", "#f59e0b"),
        Branded(ZoorceThemePreset.Ember, "Ember", "An energetic orange palette.", ThemeFamily.Warm, "#c2410c", "#ffffff", "#9a3412", "#ffedd5", "#ea580c", "#fb923c", "#321306", "#fdba74", "#4b1d08", "#f97316"),
        Branded(ZoorceThemePreset.Saffron, "Saffron", "A rich saffron-yellow palette.", ThemeFamily.Sun, "#a16207", "#ffffff", "#854d0e", "#fef3c7", "#ca8a04", "#facc15", "#2d1b00", "#fde047", "#45300b", "#fbbf24"),
        Branded(ZoorceThemePreset.Rose, "Rose", "A saturated pink palette.", ThemeFamily.Rose, "#be185d", "#ffffff", "#9d174d", "#fce7f3", "#db2777", "#f9a8d4", "#3b071d", "#fbcfe8", "#4a102d", "#f472b6"),
        Branded(ZoorceThemePreset.Obsidian, "Obsidian", "A near-black slate palette.", ThemeFamily.Stone, "#1e293b", "#ffffff", "#0f172a", "#e2e8f0", "#334155", "#cbd5e1", "#111827", "#e2e8f0", "#263143", "#94a3b8"),
        Branded(ZoorceThemePreset.Graphite, "Graphite", "A balanced gray palette.", ThemeFamily.Stone, "#475569", "#ffffff", "#334155", "#e2e8f0", "#64748b", "#b0b8c5", "#131820", "#d9e0e9", "#29323e", "#94a3b8"),
        Branded(ZoorceThemePreset.Teal, "Teal", "A composed blue-green palette.", ThemeFamily.Cool, "#0f766e", "#ffffff", "#115e59", "#ccfbf1", "#0d9488", "#5eead4", "#042f2e", "#99f6e4", "#134e4a", "#2dd4bf"),
        Branded(ZoorceThemePreset.Indigo, "Indigo", "An expressive indigo palette.", ThemeFamily.Violet, "#4338ca", "#ffffff", "#3730a3", "#e0e7ff", "#4f46e5", "#a5b4fc", "#17134b", "#c7d2fe", "#29245c", "#818cf8"),
        Branded(ZoorceThemePreset.Cyan, "Cyan", "A crisp cyan palette.", ThemeFamily.Cool, "#0e7490", "#ffffff", "#155e75", "#cffafe", "#0891b2", "#67e8f9", "#083344", "#a5f3fc", "#164e63", "#22d3ee"),
        Branded(ZoorceThemePreset.Lime, "Lime", "A bright lime palette.", ThemeFamily.Sun, "#4d7c0f", "#ffffff", "#3f6212", "#ecfccb", "#65a30d", "#bef264", "#1a2e05", "#d9f99d", "#365314", "#a3e635"),
        Branded(ZoorceThemePreset.Coral, "Coral", "A warm coral palette.", ThemeFamily.Warm, "#c2414d", "#ffffff", "#9f1239", "#ffe4e6", "#e85d75", "#fda4af", "#410913", "#fecdd3", "#5c1321", "#fb7185"),
        Branded(ZoorceThemePreset.Navy, "Navy", "A deep navy palette.", ThemeFamily.Cool, "#1e3a8a", "#ffffff", "#1e3a8a", "#dbeafe", "#2563eb", "#93c5fd", "#0b1736", "#bfdbfe", "#172554", "#60a5fa"),
        Branded(ZoorceThemePreset.Violet, "Violet", "A vivid violet palette.", ThemeFamily.Violet, "#7c3aed", "#ffffff", "#6d28d9", "#ede9fe", "#8b5cf6", "#c4b5fd", "#2e1065", "#ddd6fe", "#3b1b72", "#a78bfa"),
        Branded(ZoorceThemePreset.Magenta, "Magenta", "A high-chroma magenta palette.", ThemeFamily.Rose, "#a21caf", "#ffffff", "#86198f", "#fae8ff", "#c026d3", "#f0abfc", "#3b0764", "#f5d0fe", "#581c87", "#e879f9"),
        Branded(ZoorceThemePreset.Burgundy, "Burgundy", "A deep wine-red palette.", ThemeFamily.Rose, "#881337", "#ffffff", "#701a2f", "#ffe4e6", "#9f1239", "#fda4af", "#310b1a", "#fecdd3", "#4c1022", "#fb7185"),
        Branded(ZoorceThemePreset.Copper, "Copper", "A warm copper palette.", ThemeFamily.Warm, "#9a3412", "#ffffff", "#7c2d12", "#ffedd5", "#c2410c", "#fdba74", "#341005", "#fed7aa", "#4a1d0b", "#fb923c"),
        Branded(ZoorceThemePreset.Gold, "Gold", "A polished gold palette.", ThemeFamily.Sun, "#a16207", "#ffffff", "#854d0e", "#fef3c7", "#d97706", "#fde047", "#332100", "#fef08a", "#4d3907", "#fbbf24"),
        Branded(ZoorceThemePreset.Olive, "Olive", "An earthy olive palette.", ThemeFamily.Sun, "#5f6f10", "#ffffff", "#4d5b0c", "#f1f5d5", "#71831a", "#c9d769", "#202807", "#e2ec9d", "#3d4910", "#b8c957"),
        Branded(ZoorceThemePreset.Mint, "Mint", "A fresh mint palette.", ThemeFamily.Verdant, "#047857", "#ffffff", "#065f46", "#d1fae5", "#059669", "#6ee7b7", "#022c22", "#a7f3d0", "#064e3b", "#34d399"),
        Branded(ZoorceThemePreset.Turquoise, "Turquoise", "A bright turquoise palette.", ThemeFamily.Cool, "#087f8c", "#ffffff", "#0e7490", "#cffafe", "#06b6d4", "#5eead4", "#062f3b", "#a5f3fc", "#164e63", "#22d3ee"),
        Branded(ZoorceThemePreset.Sand, "Sand", "An understated sand palette.", ThemeFamily.Warm, "#8b5e34", "#ffffff", "#6f4a2a", "#f7eadf", "#ad7b4d", "#e9cba6", "#302015", "#f4dbc0", "#4d3421", "#d9a76b"),
        Branded(ZoorceThemePreset.Lavender, "Lavender", "A soft lavender palette.", ThemeFamily.Violet, "#7e57c2", "#ffffff", "#6941a7", "#ede9fe", "#8b6ed6", "#d8b4fe", "#2d1644", "#e9d5ff", "#452465", "#c084fc"),
        Branded(ZoorceThemePreset.Cobalt, "Cobalt", "An electric cobalt palette.", ThemeFamily.Cool, "#1d4ed8", "#ffffff", "#1e40af", "#dbeafe", "#2563eb", "#93c5fd", "#101b4e", "#bfdbfe", "#1e3a8a", "#60a5fa"),
        Branded(ZoorceThemePreset.Forest, "Forest", "A deep forest-green palette.", ThemeFamily.Verdant, "#166534", "#ffffff", "#14532d", "#dcfce7", "#15803d", "#86efac", "#082915", "#bbf7d0", "#14532d", "#4ade80"),
        Branded(ZoorceThemePreset.Raspberry, "Raspberry", "A rich raspberry palette.", ThemeFamily.Rose, "#9d174d", "#ffffff", "#831843", "#fce7f3", "#be185d", "#f9a8d4", "#36091e", "#fbcfe8", "#500724", "#f472b6"),
        Branded(ZoorceThemePreset.Stone, "Stone", "A warm mineral-gray palette.", ThemeFamily.Stone, "#57534e", "#ffffff", "#44403c", "#ece9e3", "#78716c", "#d6d3d1", "#1c1917", "#e7e5e4", "#292524", "#a8a29e")
    ];

    private static readonly IReadOnlyDictionary<ZoorceThemePreset, PresetDefinition> DefinitionsByPreset = Definitions.ToDictionary(definition => definition.Info.Preset);
    private static readonly IReadOnlyList<ZoorceThemePresetInfo> AvailablePresets = Definitions.Select(definition => definition.Info).ToArray();

    /// <summary>Gets every built-in theme in stable display order.</summary>
    public static IReadOnlyList<ZoorceThemePresetInfo> Available => AvailablePresets;

    /// <summary>Applies a complete preset to both configured color modes.</summary>
    public static void Apply(ZoorceThemeCatalogOptions options, ZoorceThemePreset preset)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Light.CopyFrom(CreatePalette(preset, ZoorceThemeMode.Light));
        options.Dark.CopyFrom(CreatePalette(preset, ZoorceThemeMode.Dark));
    }

    /// <summary>Creates an independent palette for one preset and color mode.</summary>
    public static ZoorceThemePalette CreatePalette(ZoorceThemePreset preset, ZoorceThemeMode mode)
    {
        if (!DefinitionsByPreset.TryGetValue(preset, out var definition))
        {
            throw new ArgumentOutOfRangeException(nameof(preset), preset, "The theme preset is not supported.");
        }

        return mode switch
        {
            ZoorceThemeMode.Light => definition.CreateLight(),
            ZoorceThemeMode.Dark => definition.CreateDark(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The theme mode is not supported.")
        };
    }

    private static PresetDefinition Branded(ZoorceThemePreset preset, string displayName, string description, ThemeFamily family, string lightPrimary, string lightForeground, string lightHover, string lightSoft, string lightDataPrimary, string darkPrimary, string darkForeground, string darkHover, string darkSoft, string darkDataPrimary)
    {
        var recipe = new BrandRecipe(family, lightPrimary, lightForeground, lightHover, lightSoft, lightDataPrimary, darkPrimary, darkForeground, darkHover, darkSoft, darkDataPrimary);
        return new PresetDefinition(new ZoorceThemePresetInfo(preset, displayName, description, lightPrimary, darkPrimary), () => CreateBrandedPalette(recipe, ZoorceThemeMode.Light), () => CreateBrandedPalette(recipe, ZoorceThemeMode.Dark));
    }

    private static ZoorceThemePalette CreateBrandedPalette(BrandRecipe recipe, ZoorceThemeMode mode)
    {
        var palette = CreateFamilyPalette(recipe.Family, mode);
        var primary = mode == ZoorceThemeMode.Light ? recipe.LightPrimary : recipe.DarkPrimary;
        palette.Primary = primary;
        palette.PrimaryForeground = mode == ZoorceThemeMode.Light ? recipe.LightForeground : recipe.DarkForeground;
        palette.PrimaryHover = mode == ZoorceThemeMode.Light ? recipe.LightHover : recipe.DarkHover;
        palette.PrimarySoft = mode == ZoorceThemeMode.Light ? recipe.LightSoft : recipe.DarkSoft;
        palette.DataPrimary = mode == ZoorceThemeMode.Light ? recipe.LightDataPrimary : recipe.DarkDataPrimary;
        palette.Glow = $"color-mix(in srgb, {primary} {(mode == ZoorceThemeMode.Light ? 13 : 24)}%, transparent)";
        return palette;
    }

    private static ZoorceThemePalette CreateFamilyPalette(ThemeFamily family, ZoorceThemeMode mode) => (family, mode) switch
    {
        (ThemeFamily.Verdant, ZoorceThemeMode.Light) => ZoorceThemePalette.CreateLight(),
        (ThemeFamily.Verdant, ZoorceThemeMode.Dark) => ZoorceThemePalette.CreateDark(),
        (ThemeFamily.Violet, ZoorceThemeMode.Light) => CreateAmethystLight(),
        (ThemeFamily.Violet, ZoorceThemeMode.Dark) => CreateAmethystDark(),
        (ThemeFamily.Cool, ZoorceThemeMode.Light) => CreateNeutralLight(),
        (ThemeFamily.Cool, ZoorceThemeMode.Dark) => CreateNeutralDark(),
        (ThemeFamily.Warm, ZoorceThemeMode.Light) => CreateWarmLight(),
        (ThemeFamily.Warm, ZoorceThemeMode.Dark) => CreateWarmDark(),
        (ThemeFamily.Rose, ZoorceThemeMode.Light) => CreateRoseLight(),
        (ThemeFamily.Rose, ZoorceThemeMode.Dark) => CreateRoseDark(),
        (ThemeFamily.Sun, ZoorceThemeMode.Light) => CreateSunLight(),
        (ThemeFamily.Sun, ZoorceThemeMode.Dark) => CreateSunDark(),
        (ThemeFamily.Stone, ZoorceThemeMode.Light) => CreateStoneLight(),
        (ThemeFamily.Stone, ZoorceThemeMode.Dark) => CreateStoneDark(),
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "The theme family is not supported.")
    };

    private static ZoorceThemePalette CreateWarmLight() => Customize(ZoorceThemePalette.CreateLight(), "#fcf8f4", "#f5ede4", "#2d2118", "#756358", "#e7d8c8", "#f8efe6", "#432b1b", "#7c6551", "rgba(69, 38, 19, 0.14)", "#17100c", "#ffe1c2", "#5c3821", "#291a11", "#e6bb94", "rgba(25, 13, 7, 0.58)");
    private static ZoorceThemePalette CreateWarmDark() => Customize(ZoorceThemePalette.CreateDark(), "#120d09", "#34241a", "#fff5ed", "#c3ae9c", "#513828", "#17100c", "#fff5ed", "#bea58e", "rgba(0, 0, 0, 0.50)", "#130d09", "#ffe1c2", "#5c3821", "#281911", "#e6bb94", "rgba(8, 5, 3, 0.66)", "#1c1510", "#271b14");
    private static ZoorceThemePalette CreateRoseLight() => Customize(ZoorceThemePalette.CreateLight(), "#fff8fb", "#fdf0f5", "#301825", "#765767", "#ebd4df", "#fff0f6", "#4a1830", "#8a5b72", "rgba(91, 24, 55, 0.14)", "#1c0c14", "#ffd9e9", "#5e2440", "#301220", "#e4a8c2", "rgba(29, 5, 17, 0.58)");
    private static ZoorceThemePalette CreateRoseDark() => Customize(ZoorceThemePalette.CreateDark(), "#140a10", "#351a28", "#fff1f6", "#d0aeba", "#51253b", "#180b13", "#fff1f6", "#c7a0b1", "rgba(0, 0, 0, 0.50)", "#170911", "#ffd9e9", "#5e2440", "#2d1020", "#e4a8c2", "rgba(9, 3, 6, 0.67)", "#1e0f18", "#29131f");
    private static ZoorceThemePalette CreateSunLight() => Customize(ZoorceThemePalette.CreateLight(), "#fffdf5", "#f8f2df", "#332914", "#706547", "#e9ddbd", "#faf4df", "#4a3a12", "#7d6c3e", "rgba(92, 66, 14, 0.13)", "#171307", "#fff0ad", "#5b4b1c", "#2b240d", "#dfcf7b", "rgba(22, 17, 3, 0.56)");
    private static ZoorceThemePalette CreateSunDark() => Customize(ZoorceThemePalette.CreateDark(), "#121005", "#342f17", "#fffbe7", "#c6bd93", "#514a26", "#161306", "#fffbe7", "#c0b57e", "rgba(0, 0, 0, 0.50)", "#141106", "#fff0ad", "#5b4b1c", "#28220d", "#dfcf7b", "rgba(8, 7, 2, 0.67)", "#1b180a", "#262211");
    private static ZoorceThemePalette CreateStoneLight() => Customize(ZoorceThemePalette.CreateLight(), "#fafaf9", "#f1f0ee", "#292524", "#69625e", "#ddd9d4", "#f3f1ee", "#322e2a", "#746d67", "rgba(37, 34, 31, 0.14)", "#161514", "#e6e1dc", "#4d4843", "#24211f", "#c4bdb5", "rgba(14, 13, 12, 0.58)");
    private static ZoorceThemePalette CreateStoneDark() => Customize(ZoorceThemePalette.CreateDark(), "#11100f", "#2c2926", "#f4f1ed", "#b7b0a9", "#403c38", "#151311", "#f4f1ed", "#aea69e", "rgba(0, 0, 0, 0.52)", "#131211", "#e6e1dc", "#4d4843", "#211e1c", "#c4bdb5", "rgba(5, 5, 4, 0.67)", "#191716", "#211f1d");

    private static ZoorceThemePalette Customize(ZoorceThemePalette palette, string canvas, string subtle, string text, string muted, string border, string sidebar, string sidebarText, string sidebarMuted, string shadow, string codeCanvas, string codeText, string codeBorder, string codeControlSurface, string codeControlText, string overlay, string? surface = null, string? elevated = null)
    {
        palette.Canvas = canvas; palette.Surface = surface ?? palette.Surface; palette.Elevated = elevated ?? palette.Elevated; palette.Subtle = subtle; palette.Text = text; palette.Muted = muted; palette.Border = border;
        palette.Sidebar = sidebar; palette.SidebarText = sidebarText; palette.SidebarMuted = sidebarMuted; palette.Shadow = shadow;
        palette.CodeCanvas = codeCanvas; palette.CodeText = codeText; palette.CodeBorder = codeBorder; palette.CodeControlSurface = codeControlSurface; palette.CodeControlText = codeControlText; palette.Overlay = overlay;
        return palette;
    }

    private static ZoorceThemePalette CreateAmethystLight() => new()
    {
        Canvas = "#faf8ff", Surface = "#ffffff", Elevated = "#ffffff", Subtle = "#f1edff", Text = "#211b2e", Muted = "#665d78", Border = "#ddd6ee", Primary = "#6d28d9", PrimaryForeground = "#ffffff", PrimaryHover = "#5b21b6", PrimarySoft = "#ede9fe", Success = "#087a57", SuccessSoft = "#ddf4e9", Warning = "#8a5a00", WarningSoft = "#fff1d6", Danger = "#b4233f", DangerForeground = "#ffffff", DangerSoft = "#ffe7ec", Info = "#285eb8", InfoSoft = "#e6eeff", DataPrimary = "#2563c9", DataSecondary = "#7c3aed", DataTertiary = "#c026d3", DataQuaternary = "#d97706", DataQuinary = "#0f766e", Sidebar = "#f3efff", SidebarText = "#2d2145", SidebarMuted = "#74668c", Glow = "rgba(109, 40, 217, 0.10)", Shadow = "rgba(46, 30, 80, 0.14)", CodeCanvas = "#171022", CodeText = "#ece5ff", CodeBorder = "#443461", CodeControlSurface = "#261a37", CodeControlText = "#c9b8ff", Overlay = "rgba(26, 16, 43, 0.56)"
    };

    private static ZoorceThemePalette CreateAmethystDark() => new()
    {
        Canvas = "#0d0a13", Surface = "#16101f", Elevated = "#1d1529", Subtle = "#281d37", Text = "#f7f2ff", Muted = "#b9afc9", Border = "#39294d", Primary = "#a78bfa", PrimaryForeground = "#160b2c", PrimaryHover = "#c4b5fd", PrimarySoft = "#332252", Success = "#39d98a", SuccessSoft = "#123d2b", Warning = "#f2bd58", WarningSoft = "#3a2c12", Danger = "#ff8291", DangerForeground = "#29070d", DangerSoft = "#491720", Info = "#94b6ff", InfoSoft = "#1c2b52", DataPrimary = "#7fb0ff", DataSecondary = "#b494ff", DataTertiary = "#f08dde", DataQuaternary = "#ffc86a", DataQuinary = "#56d5c7", Sidebar = "#120d1b", SidebarText = "#f7f2ff", SidebarMuted = "#aca0c3", Glow = "rgba(167, 139, 250, 0.24)", Shadow = "rgba(0, 0, 0, 0.48)", CodeCanvas = "#110b1a", CodeText = "#e9ddff", CodeBorder = "#4b3966", CodeControlSurface = "#24172f", CodeControlText = "#d0bdff", Overlay = "rgba(7, 4, 11, 0.62)"
    };

    private static ZoorceThemePalette CreateNeutralLight() => new()
    {
        Canvas = "#f7f8fb", Surface = "#ffffff", Elevated = "#ffffff", Subtle = "#eef1f6", Text = "#18202f", Muted = "#5d687a", Border = "#d8dee9", Primary = "#4056c8", PrimaryForeground = "#ffffff", PrimaryHover = "#3045ad", PrimarySoft = "#e5e9ff", Success = "#087a57", SuccessSoft = "#ddf4e9", Warning = "#8a5a00", WarningSoft = "#fff1d6", Danger = "#b4233f", DangerForeground = "#ffffff", DangerSoft = "#ffe7ec", Info = "#285eb8", InfoSoft = "#e6eeff", DataPrimary = "#2563c9", DataSecondary = "#7253c6", DataTertiary = "#b9369d", DataQuaternary = "#c57608", DataQuinary = "#087d75", Sidebar = "#f1f4f9", SidebarText = "#1d2b42", SidebarMuted = "#62708a", Glow = "rgba(64, 86, 200, 0.09)", Shadow = "rgba(24, 32, 47, 0.13)", CodeCanvas = "#121a29", CodeText = "#dce6ff", CodeBorder = "#34445f", CodeControlSurface = "#1b2940", CodeControlText = "#aec7ff", Overlay = "rgba(16, 23, 35, 0.54)"
    };

    private static ZoorceThemePalette CreateNeutralDark() => new()
    {
        Canvas = "#0c111a", Surface = "#121a26", Elevated = "#182235", Subtle = "#202d43", Text = "#eef4ff", Muted = "#aab6c9", Border = "#2c3b55", Primary = "#91a7ff", PrimaryForeground = "#111936", PrimaryHover = "#bac7ff", PrimarySoft = "#26386c", Success = "#39d98a", SuccessSoft = "#123d2b", Warning = "#f2bd58", WarningSoft = "#3a2c12", Danger = "#ff8291", DangerForeground = "#29070d", DangerSoft = "#491720", Info = "#9fc0ff", InfoSoft = "#1d3159", DataPrimary = "#7fb0ff", DataSecondary = "#b89dff", DataTertiary = "#ed91d5", DataQuaternary = "#ffd06c", DataQuinary = "#5dd3c4", Sidebar = "#101824", SidebarText = "#eef4ff", SidebarMuted = "#9daec8", Glow = "rgba(145, 167, 255, 0.22)", Shadow = "rgba(0, 0, 0, 0.48)", CodeCanvas = "#0a101a", CodeText = "#dbe7ff", CodeBorder = "#32415a", CodeControlSurface = "#152237", CodeControlText = "#b8ceff", Overlay = "rgba(4, 8, 14, 0.62)"
    };

    private sealed record PresetDefinition(ZoorceThemePresetInfo Info, Func<ZoorceThemePalette> CreateLight, Func<ZoorceThemePalette> CreateDark);
    private sealed record BrandRecipe(ThemeFamily Family, string LightPrimary, string LightForeground, string LightHover, string LightSoft, string LightDataPrimary, string DarkPrimary, string DarkForeground, string DarkHover, string DarkSoft, string DarkDataPrimary);

    private enum ThemeFamily { Verdant, Violet, Cool, Warm, Rose, Sun, Stone }
}
