namespace Zoorce.Web.UI.Charts;

/// <summary>Configures scales, grid lines and labels for charts that expose axes.</summary>
public sealed record ZoorceChartAxesOptions
{
    /// <summary>Gets whether axes are rendered where the selected chart kind supports them.</summary>
    public bool Visible { get; init; } = true;

    /// <summary>Gets whether value-scale grid lines are rendered.</summary>
    public bool ShowGridLines { get; init; } = true;

    /// <summary>Gets whether numeric or date value labels are rendered.</summary>
    public bool ShowValueLabels { get; init; } = true;

    /// <summary>Gets whether category labels are rendered.</summary>
    public bool ShowCategoryLabels { get; init; } = true;

    /// <summary>Gets the preferred number of value-scale intervals.</summary>
    public int ValueTickCount { get; init; } = 4;

    /// <summary>Gets the category-label interval; zero uses an automatic interval.</summary>
    public int CategoryLabelInterval { get; init; }

    /// <summary>Returns an axes configuration bounded to values the renderer can display.</summary>
    public ZoorceChartAxesOptions Normalize() => this with
    {
        ValueTickCount = Math.Clamp(ValueTickCount, 2, 12),
        CategoryLabelInterval = Math.Max(0, CategoryLabelInterval)
    };
}
