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

    /// <summary>Gets or sets the foreground color placed on the primary brand accent.</summary>
    public string PrimaryForeground { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary accent hover color.</summary>
    public string PrimaryHover { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis primary accent.</summary>
    public string PrimarySoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the positive semantic accent.</summary>
    public string Success { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis positive surface.</summary>
    public string SuccessSoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the warning semantic accent.</summary>
    public string Warning { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis warning surface.</summary>
    public string WarningSoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the danger semantic accent.</summary>
    public string Danger { get; set; } = string.Empty;

    /// <summary>Gets or sets the foreground color placed on the danger accent.</summary>
    public string DangerForeground { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis danger surface.</summary>
    public string DangerSoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the information semantic accent.</summary>
    public string Info { get; set; } = string.Empty;

    /// <summary>Gets or sets the low-emphasis information surface.</summary>
    public string InfoSoft { get; set; } = string.Empty;

    /// <summary>Gets or sets the secondary data-visualization accent.</summary>
    public string DataSecondary { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar background color.</summary>
    public string Sidebar { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar primary content color.</summary>
    public string SidebarText { get; set; } = string.Empty;

    /// <summary>Gets or sets the sidebar lower-emphasis content color.</summary>
    public string SidebarMuted { get; set; } = string.Empty;

    /// <summary>Gets or sets the shadow and ambient-glow color.</summary>
    public string Glow { get; set; } = string.Empty;

    /// <summary>Gets or sets the neutral shadow color used by elevated surfaces.</summary>
    public string Shadow { get; set; } = string.Empty;

    /// <summary>Gets or sets the background used by rendered code blocks.</summary>
    public string CodeCanvas { get; set; } = string.Empty;

    /// <summary>Gets or sets the foreground used by rendered code blocks.</summary>
    public string CodeText { get; set; } = string.Empty;

    /// <summary>Gets or sets the border used by rendered code controls.</summary>
    public string CodeBorder { get; set; } = string.Empty;

    /// <summary>Gets or sets the surface used by rendered code controls.</summary>
    public string CodeControlSurface { get; set; } = string.Empty;

    /// <summary>Gets or sets the foreground used by rendered code controls.</summary>
    public string CodeControlText { get; set; } = string.Empty;

    /// <summary>Gets or sets the scrim shown behind modal and mobile overlay content.</summary>
    public string Overlay { get; set; } = string.Empty;

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
            Variable("--z-primary-foreground", PrimaryForeground),
            Variable("--z-primary-hover", PrimaryHover),
            Variable("--z-primary-soft", PrimarySoft),
            Variable("--z-success", Success),
            Variable("--z-success-soft", SuccessSoft),
            Variable("--z-warning", Warning),
            Variable("--z-warning-soft", WarningSoft),
            Variable("--z-danger", Danger),
            Variable("--z-danger-foreground", DangerForeground),
            Variable("--z-danger-soft", DangerSoft),
            Variable("--z-info", Info),
            Variable("--z-info-soft", InfoSoft),
            Variable("--z-data-secondary", DataSecondary),
            Variable("--z-sidebar", Sidebar),
            Variable("--z-sidebar-text", SidebarText),
            Variable("--z-sidebar-muted", SidebarMuted),
            Variable("--z-glow", Glow),
            Variable("--z-shadow", Shadow),
            Variable("--z-code-canvas", CodeCanvas),
            Variable("--z-code-text", CodeText),
            Variable("--z-code-border", CodeBorder),
            Variable("--z-code-control-surface", CodeControlSurface),
            Variable("--z-code-control-text", CodeControlText),
            Variable("--z-overlay", Overlay)
        ]);

    /// <summary>Copies every semantic token from another palette.</summary>
    public void CopyFrom(ZoorceThemePaletteOptions source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Canvas = source.Canvas;
        Surface = source.Surface;
        Elevated = source.Elevated;
        Subtle = source.Subtle;
        Text = source.Text;
        Muted = source.Muted;
        Border = source.Border;
        Primary = source.Primary;
        PrimaryForeground = source.PrimaryForeground;
        PrimaryHover = source.PrimaryHover;
        PrimarySoft = source.PrimarySoft;
        Success = source.Success;
        SuccessSoft = source.SuccessSoft;
        Warning = source.Warning;
        WarningSoft = source.WarningSoft;
        Danger = source.Danger;
        DangerForeground = source.DangerForeground;
        DangerSoft = source.DangerSoft;
        Info = source.Info;
        InfoSoft = source.InfoSoft;
        DataSecondary = source.DataSecondary;
        Sidebar = source.Sidebar;
        SidebarText = source.SidebarText;
        SidebarMuted = source.SidebarMuted;
        Glow = source.Glow;
        Shadow = source.Shadow;
        CodeCanvas = source.CodeCanvas;
        CodeText = source.CodeText;
        CodeBorder = source.CodeBorder;
        CodeControlSurface = source.CodeControlSurface;
        CodeControlText = source.CodeControlText;
        Overlay = source.Overlay;
    }

    /// <summary>
    /// Creates the default light palette.
    /// </summary>
    public static ZoorceThemePaletteOptions CreateLight() => new()
    {
        Canvas = "#f6f8f7",
        Surface = "#ffffff",
        Elevated = "#ffffff",
        Subtle = "#edf3f0",
        Text = "#15251f",
        Muted = "#52675e",
        Border = "#d6e2dc",
        Primary = "#087a57",
        PrimaryForeground = "#ffffff",
        PrimaryHover = "#056446",
        PrimarySoft = "#d9f2e7",
        Success = "#087a57",
        SuccessSoft = "#ddf4e9",
        Warning = "#8a5a00",
        WarningSoft = "#fff1d6",
        Danger = "#b4233f",
        DangerForeground = "#ffffff",
        DangerSoft = "#ffe7ec",
        Info = "#285eb8",
        InfoSoft = "#e6eeff",
        DataSecondary = "#6556bb",
        Sidebar = "#eef5f1",
        SidebarText = "#17362a",
        SidebarMuted = "#587267",
        Glow = "rgba(8, 122, 87, 0.08)",
        Shadow = "rgba(19, 54, 40, 0.14)",
        CodeCanvas = "#06100d",
        CodeText = "#baf7d9",
        CodeBorder = "#245343",
        CodeControlSurface = "#0e211b",
        CodeControlText = "#9bd9ba",
        Overlay = "rgba(1, 12, 9, 0.56)"
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
        PrimaryForeground = "#04100b",
        PrimaryHover = "#53f3be",
        PrimarySoft = "#123f31",
        Success = "#20e7a0",
        SuccessSoft = "#123f31",
        Warning = "#f2bd58",
        WarningSoft = "#3a2c12",
        Danger = "#ff7586",
        DangerForeground = "#220509",
        DangerSoft = "#40171e",
        Info = "#8eb0ff",
        InfoSoft = "#192b50",
        DataSecondary = "#a18bff",
        Sidebar = "#07120f",
        SidebarText = "#eefff8",
        SidebarMuted = "#83a89a",
        Glow = "rgba(32, 231, 160, 0.24)",
        Shadow = "rgba(0, 0, 0, 0.44)",
        CodeCanvas = "#06100d",
        CodeText = "#baf7d9",
        CodeBorder = "#245343",
        CodeControlSurface = "#0e211b",
        CodeControlText = "#9bd9ba",
        Overlay = "rgba(0, 0, 0, 0.56)"
    };

    private static string Variable(string name, string value) => $"{name}:{value};";
}
