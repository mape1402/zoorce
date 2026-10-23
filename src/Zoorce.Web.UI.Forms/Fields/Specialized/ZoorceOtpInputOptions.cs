namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a segmented verification-code field.</summary>
public sealed record ZoorceOtpInputOptions : ZoorceFieldOptions
{
    /// <summary>Gets the number of visible code cells.</summary>
    public int CodeLength { get; init; } = 6;

    /// <summary>Gets the allowed character set.</summary>
    public ZoorceOtpCharacterSet CharacterSet { get; init; } = ZoorceOtpCharacterSet.Numeric;

    /// <summary>Gets the case normalization applied to letters.</summary>
    public ZoorceOtpTextTransform TextTransform { get; init; } = ZoorceOtpTextTransform.Uppercase;

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => base.CreateRenderOptions() with
        {
            Kind = ZoorceFormControlKind.Otp,
            CodeLength = CodeLength,
            OtpCharacterSet = CharacterSet,
            OtpTextTransform = TextTransform
        };
}
