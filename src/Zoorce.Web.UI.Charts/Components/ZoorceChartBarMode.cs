namespace Zoorce.Web.UI.Charts;

/// <summary>Defines how multiple categorical bar series are composed.</summary>
public enum ZoorceChartBarMode
{
    /// <summary>Places each series beside its peers.</summary>
    Grouped,

    /// <summary>Stacks each series into one category total.</summary>
    Stacked,

    /// <summary>Normalizes each category to one hundred percent.</summary>
    Stacked100
}
