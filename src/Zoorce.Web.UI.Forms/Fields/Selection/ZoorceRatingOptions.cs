namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a rating selector.</summary>
public sealed record ZoorceRatingOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets whether clicking the current rating clears it.</summary>
    public bool AllowClear { get; init; }

    /// <summary>Gets whether the selected rating fills all preceding icons or only the selected icon.</summary>
    public ZoorceRatingDisplayMode DisplayMode { get; init; } = ZoorceRatingDisplayMode.Cumulative;

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with
        {
            Kind = ZoorceFormControlKind.Rating,
            AllowClear = AllowClear,
            RatingDisplayMode = DisplayMode
        };
}
