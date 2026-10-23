namespace Zoorce.Web.UI.Abstractions.Data;

/// <summary>
/// Configures the default loading experience for an isolated data-bound component.
/// </summary>
public sealed record ZoorceLoadingOptions
{
    /// <summary>Gets the shared default that enables loading feedback.</summary>
    public static ZoorceLoadingOptions Default { get; } = new();

    /// <summary>Gets whether loading feedback is rendered.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Gets the visual used while the first result is pending.</summary>
    public ZoorceLoadVisual InitialLoadVisual { get; init; } = ZoorceLoadVisual.Auto;

    /// <summary>Gets the visual used while existing content refreshes.</summary>
    public ZoorceLoadVisual RefreshLoadVisual { get; init; } = ZoorceLoadVisual.Progress;

    /// <summary>Gets whether the previous result remains visible during a refresh.</summary>
    public bool KeepPreviousContent { get; init; } = true;

    /// <summary>Gets the minimum delay before indeterminate feedback is shown.</summary>
    public TimeSpan DisplayDelay { get; init; } = TimeSpan.FromMilliseconds(120);

    /// <summary>Returns a normalized immutable loading configuration.</summary>
    public ZoorceLoadingOptions Normalize()
        => this with
        {
            InitialLoadVisual = Enum.IsDefined(InitialLoadVisual) ? InitialLoadVisual : ZoorceLoadVisual.Auto,
            RefreshLoadVisual = Enum.IsDefined(RefreshLoadVisual) ? RefreshLoadVisual : ZoorceLoadVisual.Progress,
            DisplayDelay = DisplayDelay < TimeSpan.Zero ? TimeSpan.Zero : DisplayDelay
        };
}
