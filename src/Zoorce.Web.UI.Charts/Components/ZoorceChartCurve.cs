namespace Zoorce.Web.UI.Charts;

/// <summary>Defines the interpolation used by trend-style charts.</summary>
public enum ZoorceChartCurve
{
    /// <summary>Connects observations with direct segments.</summary>
    Straight,

    /// <summary>Uses a restrained monotone-like smooth curve.</summary>
    Smooth,

    /// <summary>Uses horizontal and vertical steps between observations.</summary>
    Step
}
