namespace Zoorce.Web.UI.Charts;

/// <summary>Defines how an individual series is rendered inside a combo chart.</summary>
public enum ZoorceChartSeriesKind
{
    /// <summary>Lets the chart select the default representation.</summary>
    Auto,

    /// <summary>Renders a connected line.</summary>
    Line,

    /// <summary>Renders a filled trend.</summary>
    Area,

    /// <summary>Renders vertical bars.</summary>
    Bar,

    /// <summary>Renders stems with endpoint markers.</summary>
    Lollipop
}
