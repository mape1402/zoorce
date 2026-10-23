namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a percentage numeric input.</summary>
public sealed record ZoorcePercentageInputOptions : ZoorceNumericFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.Percentage };
}
