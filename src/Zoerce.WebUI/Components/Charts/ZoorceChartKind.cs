namespace Zoerce.WebUI.Components.Charts;

/// <summary>Identifies the native SVG visualization rendered by a Zoorce chart.</summary>
public enum ZoorceChartKind
{
    /// <summary>A connected value trend.</summary>
    Line,

    /// <summary>A filled value trend.</summary>
    Area,

    /// <summary>Vertical value bars.</summary>
    Bar,

    /// <summary>A proportional ring.</summary>
    Donut,

    /// <summary>A compact trend without axes.</summary>
    Sparkline
}
