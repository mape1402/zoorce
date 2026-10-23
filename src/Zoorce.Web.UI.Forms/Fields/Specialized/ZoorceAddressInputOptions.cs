namespace Zoorce.Web.UI.Forms;

/// <summary>Configures an address input with country choices.</summary>
public sealed record ZoorceAddressInputOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets the initially selected country code.</summary>
    public string? CountryCode { get; init; }

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.Address, Metadata = WithMetadata("country", CountryCode) };

    private IReadOnlyDictionary<string, string?> WithMetadata(string key, string? value)
        => Metadata.Concat(new[] { new KeyValuePair<string, string?>(key, value) }).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
}
