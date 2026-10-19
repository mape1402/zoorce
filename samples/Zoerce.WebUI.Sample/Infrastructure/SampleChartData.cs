namespace Zoerce.WebUI.Sample.Infrastructure;

public static class SampleChartData
{
    public static object Create() => new
    {
        Series = new[]
        {
            new
            {
                Name = "Adoption",
                Points = Enumerable.Range(1, 12).Select(index => new { Label = $"W{index}", Value = 42 + index * 4 + Random.Shared.Next(0, 8) })
            },
            new
            {
                Name = "Quality",
                Points = Enumerable.Range(1, 12).Select(index => new { Label = $"W{index}", Value = 35 + index * 3 + Random.Shared.Next(0, 7) })
            }
        }
    };
}
