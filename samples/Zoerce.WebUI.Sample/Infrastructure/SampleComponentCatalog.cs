using Zoerce.WebUI.Abstractions.Data;

namespace Zoerce.WebUI.Sample.Infrastructure;

public sealed class SampleComponentCatalog : IZoorceDataSource<ComponentCatalogItem>
{
    private readonly object sync = new();
    private readonly List<ComponentCatalogItem> items =
    [
        new("shell", "Application shell", "Layout", "Dark / Light", "Stable", 92),
        new("grid", "Data grid", "Data", "Dark / Light", "Stable", 87),
        new("chart", "Native SVG charts", "Data", "Dark / Light", "Preview", 74),
        new("markdown", "Markdown reader", "Content", "Dark / Light", "Preview", 68),
        new("toast", "Toast service", "Feedback", "Dark / Light", "Stable", 95),
        new("modal", "Modal dialog", "Feedback", "Dark / Light", "Stable", 83),
        new("drawer", "Context drawer", "Feedback", "Dark / Light", "Stable", 79),
        new("tabs", "Tabs", "Navigation", "Dark / Light", "Stable", 88),
        new("command", "Command palette", "Navigation", "Dark / Light", "Preview", 63),
        new("forms", "Validated fields", "Forms", "Dark / Light", "Stable", 90),
        new("empty", "Empty state", "Feedback", "Dark / Light", "Stable", 76),
        new("skeleton", "Loading skeleton", "Feedback", "Dark / Light", "Stable", 72),
        new("tooltip", "Tooltip", "Feedback", "Dark / Light", "Stable", 81),
        new("navigation", "Nested navigation", "Navigation", "Dark / Light", "Stable", 86),
        new("theme", "Theme presets", "Foundation", "Dark / Light", "Stable", 94),
        new("tokens", "Semantic tokens", "Foundation", "Dark / Light", "Stable", 97),
        new("progress", "Progress indicator", "Feedback", "Dark / Light", "Stable", 77),
        new("badge", "Status badges", "Feedback", "Dark / Light", "Stable", 84),
        new("table", "Responsive table", "Data", "Dark / Light", "Preview", 60),
        new("realtime", "Realtime adapter", "Data", "Dark / Light", "Preview", 66)
    ];

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
