using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleComponentCatalog : IZoorceDataSource<ComponentCatalogItem>
{
    private readonly object sync = new();
    private readonly List<ComponentCatalogItem> items = CreateItems();

    private static List<ComponentCatalogItem> CreateItems()
    {
        var blueprints = new (string Slug, string Name, string Category, string Status, int Adoption)[]
        {
            ("shell", "Application shell", "Layout", "Stable", 92),
            ("grid", "Data grid", "Data", "Stable", 87),
            ("chart", "Native SVG charts", "Data", "Preview", 74),
            ("markdown", "Markdown reader", "Content", "Preview", 68),
            ("toast", "Toast service", "Feedback", "Stable", 95),
            ("modal", "Modal dialog", "Feedback", "Stable", 83),
            ("drawer", "Context drawer", "Feedback", "Stable", 79),
            ("tabs", "Tabs", "Navigation", "Stable", 88),
            ("command", "Command palette", "Navigation", "Preview", 63),
            ("forms", "Validated fields", "Forms", "Stable", 90),
            ("empty", "Empty state", "Feedback", "Stable", 76),
            ("skeleton", "Loading skeleton", "Feedback", "Stable", 72),
            ("tooltip", "Tooltip", "Feedback", "Stable", 81),
            ("navigation", "Nested navigation", "Navigation", "Stable", 86),
            ("theme", "Theme presets", "Foundation", "Stable", 94),
            ("tokens", "Semantic tokens", "Foundation", "Stable", 97),
            ("progress", "Progress indicator", "Feedback", "Stable", 77),
            ("badge", "Status badges", "Feedback", "Stable", 84),
            ("table", "Responsive table", "Data", "Preview", 60),
            ("realtime", "Realtime adapter", "Data", "Preview", 66)
        };

        return Enumerable.Range(0, 14)
            .SelectMany(batch => blueprints.Select((item, index) => new ComponentCatalogItem(
                $"{item.Slug}-{batch + 1:00}",
                batch == 0 ? item.Name : $"{item.Name} {batch + 1:00}",
                item.Category,
                "Dark / Light",
                item.Status,
                Math.Clamp(item.Adoption - batch - (index % 4), 40, 99))))
            .ToList();
    }

    public Task<ZoorceDataPage<ComponentCatalogItem>> QueryAsync(
        ZoorceDataQuery query,
        ZoorcePageRequest pageRequest,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var request = pageRequest.Normalize();
        lock (sync)
        {
            IEnumerable<ComponentCatalogItem> filtered = items;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filtered = filtered.Where(item => $"{item.Name} {item.Category} {item.Status}".Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            }

            filtered = (query.Sort, query.Descending) switch
            {
                ("name", false) => filtered.OrderBy(item => item.Name),
                ("name", true) => filtered.OrderByDescending(item => item.Name),
                ("category", false) => filtered.OrderBy(item => item.Category),
                ("category", true) => filtered.OrderByDescending(item => item.Category),
                ("status", false) => filtered.OrderBy(item => item.Status),
                ("status", true) => filtered.OrderByDescending(item => item.Status),
                ("adoption", false) => filtered.OrderBy(item => item.Adoption),
                ("adoption", true) => filtered.OrderByDescending(item => item.Adoption),
                _ => filtered.OrderBy(item => item.Name)
            };

            var total = filtered.Count();
            var page = filtered.Skip(request.Offset).Take(request.PageSize).ToArray();
            return Task.FromResult(new ZoorceDataPage<ComponentCatalogItem>(page, total, request.Offset + page.Length < total, (request.Page + 1).ToString()));
        }
    }

    public ComponentCatalogItem AdvanceActivity()
    {
        lock (sync)
        {
            var index = Random.Shared.Next(items.Count);
            var updated = items[index] with { Adoption = Math.Min(100, items[index].Adoption + 1) };
            items[index] = updated;
            return updated;
        }
    }
}
