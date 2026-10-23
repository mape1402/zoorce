namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an international phone input.</summary>
public sealed record ZoorcePhoneInputOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets the initially selected country calling code.</summary>
    public string? CountryCode { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.Phone, Metadata = WithMetadata("country", CountryCode) };

    private IReadOnlyDictionary<string, string?> WithMetadata(string key, string? value)
        => Metadata.Concat(new[] { new KeyValuePair<string, string?>(key, value) }).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
}
