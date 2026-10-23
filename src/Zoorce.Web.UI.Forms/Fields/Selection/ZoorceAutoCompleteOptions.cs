namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a query-driven suggestion field with local or remote choices.</summary>
public sealed record ZoorceAutoCompleteOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets the optional HTTP endpoint queried with a <c>q</c> parameter.</summary>
    public string? Source { get; init; }

    /// <summary>Gets the number of typed characters required before querying.</summary>
    public int MinimumCharacters { get; init; } = 1;

    /// <summary>Gets the debounce delay used before a remote query begins.</summary>
    public int DebounceMilliseconds { get; init; } = 220;

    /// <summary>Gets copy shown when no choices match the query.</summary>
    public string EmptyResultsText { get; init; } = "No matching results.";

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.AutoComplete, EmptyResultsText = EmptyResultsText };
}
