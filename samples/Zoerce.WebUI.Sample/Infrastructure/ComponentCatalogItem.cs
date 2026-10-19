namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed record ComponentCatalogItem(
    string Id,
    string Name,
    string Category,
    string Theme,
    string Status,
    int Adoption);
