namespace Zoorce.Web.UI.Charts;

/// <summary>Defines how multiple area series share their value space.</summary>
public enum ZoorceChartAreaMode
{
    /// <summary>Draws each area from the zero baseline.</summary>
    Overlay,

    /// <summary>Accumulates each series on the preceding series.</summary>
    Stacked,

    /// <summary>Normalizes every category to one hundred percent before stacking.</summary>
    Stacked100
}
