namespace Zoorce.Web.UI.Charts;

/// <summary>Defines the interpolation used by trend-style charts.</summary>
public enum ZoorceChartCurve
{
    /// <summary>Connects observations with direct segments.</summary>
    Straight,

    /// <summary>
    /// Uses a segment-bounded smooth Bézier curve. The stroke remains within
    /// the values of each adjacent pair of observations.
    /// </summary>
    Smooth,

    /// <summary>
    /// Uses cubic monotone interpolation. This keeps the line within the bounds of
    /// adjacent observations instead of introducing visual overshoot.
    /// </summary>
    Monotone,

    /// <summary>Uses horizontal and vertical steps between observations.</summary>
    Step
}
