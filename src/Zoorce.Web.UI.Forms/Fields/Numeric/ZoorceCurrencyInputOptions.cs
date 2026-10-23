namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a currency-aware numeric input.</summary>
public sealed record ZoorceCurrencyInputOptions : ZoorceNumericFieldOptions
{
    /// <summary>Gets the locale used to format the currency value.</summary>
    public string? Locale { get; init; }

    /// <summary>Gets the three-letter ISO currency code.</summary>
    public string? CurrencyCode { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateNumericRenderOptions() with { Kind = ZoorceFormControlKind.Currency, Locale = Locale, CurrencyCode = CurrencyCode };
}
