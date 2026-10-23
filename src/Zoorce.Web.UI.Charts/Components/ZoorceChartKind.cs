namespace Zoorce.Web.UI.Charts;

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
    Sparkline,

    /// <summary>Ranks categories from left to right.</summary>
    HorizontalBar,

    /// <summary>Shows one value against a bounded radial scale.</summary>
    Gauge,

    /// <summary>Shows progressive conversion through a sequence of stages.</summary>
    Funnel,

    /// <summary>Shows intensity across a two-dimensional categorical grid.</summary>
    Heatmap,

    /// <summary>Plots numeric observations on two continuous axes.</summary>
    Scatter,

    /// <summary>Plots numeric observations with a third, size-based measure.</summary>
    Bubble,

    /// <summary>Shows an actual value, target and qualitative ranges.</summary>
    Bullet,

    /// <summary>Shows the distribution of values in adjacent bins.</summary>
    Histogram,

    /// <summary>Explains how sequential positive and negative changes produce a total.</summary>
    Waterfall
}
