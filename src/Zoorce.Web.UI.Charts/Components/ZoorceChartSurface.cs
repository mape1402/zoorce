namespace Zoorce.Web.UI.Charts;

/// <summary>Defines the visual container rendered around a chart.</summary>
public enum ZoorceChartSurface
{
    /// <summary>Uses the component card, border and padded visual area.</summary>
    Card,

    /// <summary>Renders only the chart content so the host owns its surface.</summary>
    Bare
}
