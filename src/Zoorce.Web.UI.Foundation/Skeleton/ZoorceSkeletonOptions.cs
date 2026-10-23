namespace Zoorce.Web.UI.Foundation.Skeleton;

/// <summary>
/// Configures a reusable layout-preserving skeleton placeholder.
/// </summary>
public sealed record ZoorceSkeletonOptions
{
    /// <summary>Gets the optional DOM id used to address the skeleton.</summary>
    public string? Id { get; init; }

    /// <summary>Gets the placeholder silhouette.</summary>
    public ZoorceSkeletonShape Shape { get; init; } = ZoorceSkeletonShape.Line;

    /// <summary>Gets the CSS width of each rendered placeholder.</summary>
    public string Width { get; init; } = "100%";

    /// <summary>Gets the CSS height of each rendered placeholder.</summary>
    public string? Height { get; init; }

    /// <summary>Gets the number of line or block placeholders to render.</summary>
    public int Count { get; init; } = 1;

    /// <summary>Gets whether the shimmer animation is active.</summary>
    public bool Animate { get; init; } = true;

    /// <summary>Gets the accessible loading label.</summary>
    public string AriaLabel { get; init; } = "Loading content";

    /// <summary>Returns a normalized immutable skeleton configuration.</summary>
    public ZoorceSkeletonOptions Normalize()
        => this with
        {
            Id = string.IsNullOrWhiteSpace(Id) ? null : Id.Trim(),
            Shape = Enum.IsDefined(Shape) ? Shape : ZoorceSkeletonShape.Line,
            Width = string.IsNullOrWhiteSpace(Width) ? "100%" : Width.Trim(),
            Height = string.IsNullOrWhiteSpace(Height) ? null : Height.Trim(),
            Count = Math.Clamp(Count, 1, 64),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Loading content" : AriaLabel.Trim()
        };
}
