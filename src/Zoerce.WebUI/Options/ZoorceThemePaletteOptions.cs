namespace Zoerce.WebUI.Options;

/// <summary>
/// Configures the semantic color tokens used by one Zoorce color mode.
/// </summary>
public sealed class ZoorceThemePaletteOptions
{
    /// <summary>Gets or sets the page canvas color.</summary>
    public string Canvas { get; set; } = string.Empty;

    /// <summary>Gets or sets the standard elevated surface color.</summary>
    public string Surface { get; set; } = string.Empty;

    /// <summary>Gets or sets the highest emphasis surface color.</summary>
    public string Elevated { get; set; } = string.Empty;

    /// <summary>Gets or sets the subtle contrast surface color.</summary>
    public string Subtle { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary content color.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the lower-emphasis content color.</summary>
    public string Muted { get; set; } = string.Empty;

    /// <summary>Gets or sets the standard border color.</summary>
    public string Border { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary brand accent.</summary>
    public string Primary { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary accent hover color.</summary>
    public string PrimaryHover { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis primary accent.</summary>
    public string PrimarySoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar background color.</summary>
    public string Sidebar { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar primary content color.</summary>
    public string SidebarText { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar lower-emphasis content color.</summary>
    public string SidebarMuted { get; set; } = string.Empty;

    /// <summary>Gets or sets the shadow and ambient-glow color.</summary>
    public string Glow { get; set; } = string.Empty;

    /// <summary>
    /// Returns CSS custom-property declarations for this palette.
    /// </summary>
    public string ToCssVariables()
        => string.Join(' ',
        [
            Variable("--z-canvas", Canvas),
            Variable("--z-surface", Surface),
            Variable("--z-elevated", Elevated),
            Variable("--z-subtle", Subtle),
            Variable("--z-text", Text),
            Variable("--z-muted", Muted),
            Variable("--z-border", Border),
            Variable("--z-primary", Primary),
            Variable("--z-primary-hover", PrimaryHover),
            Variable("--z-primary-soft", PrimarySoft),
            Variable("--z-sidebar", Sidebar),
            Variable("--z-sidebar-text", SidebarText),
            Variable("--z-sidebar-muted", SidebarMuted),
            Variable("--z-glow", Glow)
        ]);

    /// <summary>
    /// Creates the default light palette.
    /// </summary>
    public static ZoorceThemePaletteOptions CreateLight() => new()
    {
        Canvas = "#f4f8f7",
        Surface = "#ffffff",
        Elevated = "#ffffff",
        Subtle = "#eaf2ef",
        Text = "#10221c",
        Muted = "#5d7169",
        Border = "#d8e5df",
        Primary = "#087f5b",
        PrimaryHover = "#056746",
        PrimarySoft = "#d5f7e8",
        Sidebar = "#0b1714",
        SidebarText = "#f4fffa",
        SidebarMuted = "#9cb8ac",
        Glow = "rgba(8, 127, 91, 0.20)"
    };

    /// <summary>
    /// Creates the default dark palette.
    /// </summary>
    public static ZoorceThemePaletteOptions CreateDark() => new()
    {
        Canvas = "#050b0a",
        Surface = "#0b1613",
        Elevated = "#10201b",
        Subtle = "#132720",
        Text = "#edfff7",
        Muted = "#93aa9f",
        Border = "#1d3930",
        Primary = "#20e7a0",
        PrimaryHover = "#53f3be",
        PrimarySoft = "#123f31",
        Sidebar = "#07120f",
        SidebarText = "#eefff8",
        SidebarMuted = "#83a89a",
        Glow = "rgba(32, 231, 160, 0.24)"
    };

    private static string Variable(string name, string value) => $"{name}:{value};";
}
