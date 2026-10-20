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
                Points = new[]
                {
                    new { Label = "May", Value = 54 }, new { Label = "Jun", Value = 61 },
                    new { Label = "Jul", Value = 63 }, new { Label = "Aug", Value = 70 },
                    new { Label = "Sep", Value = 76 }, new { Label = "Oct", Value = 82 }
                }
            },
            new
            {
                Name = "Target",
                Points = new[]
                {
                    new { Label = "May", Value = 52 }, new { Label = "Jun", Value = 58 },
                    new { Label = "Jul", Value = 64 }, new { Label = "Aug", Value = 68 },
                    new { Label = "Sep", Value = 74 }, new { Label = "Oct", Value = 80 }
                }
            }
        }
    };
}
