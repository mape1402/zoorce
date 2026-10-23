namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a touch-friendly, centered quantity stepper.</summary>
public sealed record ZoorceQuantityStepperOptions : ZoorceNumericFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.QuantityStepper };
}
