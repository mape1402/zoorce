using Microsoft.AspNetCore.Razor.TagHelpers;
using Zoorce.Web.UI.TagHelpers;

namespace Zoorce.Web.UI.Tests;

public sealed class ValkyTagHelperTests
{
    [Fact]
    public async Task Renders_configurable_accessible_companion_and_encodes_message()
    {
        var helper = new ZoorceValkyTagHelper { State = "THINKING", Label = "Valky help", Message = "<hello>", Open = true, Floating = true, CssClass = "custom" };
        var output = CreateOutput();
        await helper.ProcessAsync(CreateContext(), output);
        Assert.Equal("details", output.TagName);
        Assert.Equal("thinking", output.Attributes["data-z-valky-state"].Value);
        Assert.Contains("z-valky--floating", output.Attributes["class"].Value.ToString());
        Assert.NotNull(output.Attributes["open"]);
        Assert.Contains("&lt;hello&gt;", output.Content.GetContent());
        Assert.Contains("aria-label=\"Valky help\"", output.Content.GetContent());
        Assert.Contains("/_content/Zoorce.Web.UI/images/valky.png", output.Content.GetContent());
        Assert.Contains("/_content/Zoorce.Web.UI/images/valky-body.png", output.Content.GetContent());
        Assert.Contains("/_content/Zoorce.Web.UI/js/zoorce-valky.js", output.Content.GetContent());
        Assert.Equal("once", output.Attributes["data-z-valky-celebration"].Value);
    }

    [Fact]
    public async Task Supports_aligned_custom_layers_and_looping_celebration()
    {
        var output = CreateOutput();
        var helper = new ZoorceValkyTagHelper { State = "celebrating", ImageUrl = "/head.png", BodyImageUrl = "/body.png", CelebrationLoop = true };
        await helper.ProcessAsync(CreateContext(), output);
        Assert.Contains("/head.png", output.Content.GetContent());
        Assert.Contains("/body.png", output.Content.GetContent());
        Assert.Equal("loop", output.Attributes["data-z-valky-celebration"].Value);
        Assert.Contains("z-valky-mouth-cover", output.Content.GetContent());
    }

    [Fact]
    public async Task Supports_custom_content_and_rejects_unknown_states()
    {
        var output = CreateOutput("<strong>Custom</strong>");
        await new ZoorceValkyTagHelper { Message = "fallback" }.ProcessAsync(CreateContext(), output);
        Assert.Contains("<strong>Custom</strong>", output.Content.GetContent());
        Assert.DoesNotContain("fallback", output.Content.GetContent());
        Assert.Null(output.Attributes["open"]);
        await Assert.ThrowsAsync<ArgumentException>(() => new ZoorceValkyTagHelper { State = "missing" }.ProcessAsync(CreateContext(), CreateOutput()));
    }

    private static TagHelperContext CreateContext() => new(new TagHelperAttributeList(), new Dictionary<object, object>(), "valky-test");
    private static TagHelperOutput CreateOutput(string child = "") => new("zoorce-valky", new TagHelperAttributeList(), (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(child)));
}
