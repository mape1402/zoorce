namespace Zoorce.Web.UI.Theming;

/// <summary>
/// Defines one named runtime theme as semantic CSS custom-property values.
/// </summary>
public sealed record ZoorceThemeDefinition
{
    /// <summary>Gets the name used to identify this theme.</summary>
    public string Name { get; init; } = "default";

    /// <summary>Gets the CSS custom-property values applied by this theme.</summary>
    public IReadOnlyDictionary<string, string> Tokens { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Returns a normalized theme that only exposes valid custom properties.</summary>
    public ZoorceThemeDefinition Normalize()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException("A theme requires a name.", nameof(Name));
        }

        var tokens = (Tokens ?? throw new ArgumentNullException(nameof(Tokens)))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Key.TrimStart().StartsWith("--", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value.Trim(), StringComparer.Ordinal);

        return this with { Name = Name.Trim(), Tokens = tokens };
    }
}
