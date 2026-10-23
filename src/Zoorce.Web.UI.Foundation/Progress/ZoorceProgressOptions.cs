namespace Zoorce.Web.UI.Foundation.Progress;

/// <summary>
/// Configures a reusable determinate or indeterminate Zoorce progress indicator.
/// </summary>
public sealed record ZoorceProgressOptions
{
    /// <summary>Gets the DOM id used to address the indicator.</summary>
    public string? Id { get; init; }

    /// <summary>Gets the visual geometry of the indicator.</summary>
    public ZoorceProgressKind Kind { get; init; } = ZoorceProgressKind.Linear;

    /// <summary>Gets the completion ratio from zero to one, or <see langword="null"/> for indeterminate progress.</summary>
    public double? Value { get; init; }

    /// <summary>Gets the accessible and optional visible progress label.</summary>
    public string Label { get; init; } = "Loading";

    /// <summary>Gets whether a determinate percentage is rendered next to the label.</summary>
    public bool ShowValue { get; init; }

    /// <summary>Gets the semantic tone token used for the filled indicator.</summary>
    public string Tone { get; init; } = "primary";

    /// <summary>Gets the diameter in pixels used by a circular indicator.</summary>
    public int Size { get; init; } = 32;

    /// <summary>Returns a normalized immutable progress configuration.</summary>
    public ZoorceProgressOptions Normalize()
        => this with
        {
            Id = string.IsNullOrWhiteSpace(Id) ? null : Id.Trim(),
            Kind = Enum.IsDefined(Kind) ? Kind : ZoorceProgressKind.Linear,
            Value = Value is null ? null : Math.Clamp(Value.Value, 0, 1),
            Label = string.IsNullOrWhiteSpace(Label) ? "Loading" : Label.Trim(),
            Tone = string.IsNullOrWhiteSpace(Tone) ? "primary" : Tone.Trim().ToLowerInvariant(),
            Size = Math.Clamp(Size, 16, 160)
        };
}
