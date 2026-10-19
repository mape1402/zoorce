namespace Zoerce.WebUI.Abstractions.Data;

/// <summary>
/// Describes host-owned search, filter and sort state for a reusable data component.
/// </summary>
public sealed record ZoorceDataQuery(
    string? Search = null,
    string? Sort = null,
    bool Descending = false,
    IReadOnlyDictionary<string, string?>? Filters = null);
