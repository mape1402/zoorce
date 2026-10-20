using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Zoerce.WebUI;
using Zoerce.WebUI.Markdown;
using Zoerce.WebUI.Options;
using Zoerce.WebUI.TagHelpers;

namespace Zoerce.WebUI.Tests;

public sealed class ThemeAndMarkdownTests
{
    [Theory]
    [InlineData(ZoorceThemePreset.Verdant, "#087a57", "#20e7a0")]
    [InlineData(ZoorceThemePreset.Amethyst, "#6d28d9", "#a78bfa")]
    [InlineData(ZoorceThemePreset.Neutral, "#4056c8", "#91a7ff")]
    public void Complete_theme_presets_replace_both_palettes(ZoorceThemePreset preset, string lightPrimary, string darkPrimary)
    {
        var options = new ZoerceWebUiOptions();

        var returned = options.UseThemePreset(preset);

        Assert.Same(options, returned);
        Assert.Equal(lightPrimary, options.Light.Primary);
        Assert.Equal(darkPrimary, options.Dark.Primary);
        Assert.NotEqual(options.Light.Primary, options.Light.DataPrimary);
        Assert.NotEqual(options.Dark.Primary, options.Dark.DataPrimary);
        Assert.False(string.IsNullOrWhiteSpace(options.Light.CodeCanvas));
        Assert.False(string.IsNullOrWhiteSpace(options.Dark.CodeControlText));
        Assert.False(string.IsNullOrWhiteSpace(options.Light.Overlay));
        Assert.Contains("--z-code-canvas:", options.ToCssThemeRules());
        Assert.Contains("--z-overlay:", options.ToCssThemeRules());
    }

    [Fact]
    public void Theme_preset_api_rejects_null_options_and_unknown_preset()
    {
        Assert.Throws<ArgumentNullException>(() => ZoorceThemePresets.Apply(null!, ZoorceThemePreset.Verdant));
        Assert.Throws<ArgumentOutOfRangeException>(() => ZoorceThemePresets.Apply(new ZoerceWebUiOptions(), (ZoorceThemePreset)123));
    }

    [Fact]
    public void Palette_copy_and_css_output_include_every_new_semantic_token()
    {
        var source = ZoorceThemePaletteOptions.CreateDark();
        source.Canvas = "canvas";
        source.Shadow = "shadow";
        source.CodeCanvas = "code";
        source.CodeText = "code-text";
        source.CodeBorder = "code-border";
        source.CodeControlSurface = "code-control";
        source.CodeControlText = "code-control-text";
        source.Overlay = "overlay";
        source.DataPrimary = "data-primary";
        source.DataTertiary = "data-tertiary";
        source.DataQuaternary = "data-quaternary";
        source.DataQuinary = "data-quinary";
        var target = new ZoorceThemePaletteOptions();

        target.CopyFrom(source);
        var variables = target.ToCssVariables();

        Assert.Equal("canvas", target.Canvas);
        Assert.Equal("shadow", target.Shadow);
        Assert.Equal("data-primary", target.DataPrimary);
        Assert.Equal("data-tertiary", target.DataTertiary);
        Assert.Equal("data-quaternary", target.DataQuaternary);
        Assert.Equal("data-quinary", target.DataQuinary);
        Assert.Contains("--z-shadow:shadow;", variables);
        Assert.Contains("--z-code-canvas:code;", variables);
        Assert.Contains("--z-code-text:code-text;", variables);
        Assert.Contains("--z-code-border:code-border;", variables);
        Assert.Contains("--z-code-control-surface:code-control;", variables);
        Assert.Contains("--z-code-control-text:code-control-text;", variables);
        Assert.Contains("--z-overlay:overlay;", variables);
        Assert.Contains("--z-data-primary:data-primary;", variables);
        Assert.Contains("--z-data-tertiary:data-tertiary;", variables);
        Assert.Contains("--z-data-quaternary:data-quaternary;", variables);
        Assert.Contains("--z-data-quinary:data-quinary;", variables);
        Assert.Throws<ArgumentNullException>(() => target.CopyFrom(null!));
    }

    [Fact]
    public void Markdown_renderer_generates_safe_html_and_navigation_metadata()
    {
        var renderer = GetRenderer();
        var result = renderer.Render("""
# Intro *now*
# Intro `now`
## Hello, world!
## Supplied identifier {#supplied-id}
[hash](#here) [root](/docs) [child](./child) [parent](../parent)
[web](https://example.test) [mail](mailto:hello@example.test) [bad](javascript:alert(1)) [relative](not-a-url)
<script>alert('no')</script>
""");
        var html = RenderHtml(result.Html);

        Assert.Collection(result.TableOfContents,
            heading => { Assert.Equal("intro-now", heading.Id); Assert.Equal("Intro now", heading.Text); Assert.Equal(1, heading.Level); },
            heading => { Assert.Equal("intro-now-1", heading.Id); Assert.Equal("Intro now", heading.Text); Assert.Equal(1, heading.Level); },
            heading => { Assert.Equal("hello-world", heading.Id); Assert.Equal("Hello, world!", heading.Text); Assert.Equal(2, heading.Level); },
            heading => { Assert.Equal("supplied-id", heading.Id); Assert.Equal("Supplied identifier", heading.Text); Assert.Equal(2, heading.Level); });
        Assert.Contains("href=\"#here\"", html);
        Assert.Contains("href=\"/docs\"", html);
        Assert.Contains("href=\"./child\"", html);
        Assert.Contains("href=\"../parent\"", html);
        Assert.Contains("href=\"https://example.test\"", html);
        Assert.Contains("href=\"mailto:hello@example.test\"", html);
        Assert.Contains("href=\"#\"", html);
        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Markdown_renderer_rejects_null_and_its_private_policy_handles_edge_paths()
    {
        var renderer = GetRenderer();
        Assert.Throws<ArgumentNullException>(() => renderer.Render(null!));

        var type = renderer.GetType();
        var isSafe = type.GetMethod("IsSafeLink", BindingFlags.NonPublic | BindingFlags.Static)!;
        var uniqueSlug = type.GetMethod("UniqueSlug", BindingFlags.NonPublic | BindingFlags.Static)!;
        var extractText = type.GetMethod("ExtractText", BindingFlags.NonPublic | BindingFlags.Static)!;
        var appendText = type.GetMethod("AppendInlineText", BindingFlags.NonPublic | BindingFlags.Static)!;
        var buildTableOfContents = type.GetMethod("BuildTableOfContents", BindingFlags.NonPublic | BindingFlags.Static)!;
        var used = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Assert.True((bool)isSafe.Invoke(null, [" "])!);
        Assert.True((bool)isSafe.Invoke(null, ["#fragment"])!);
        Assert.True((bool)isSafe.Invoke(null, ["/root"])!);
        Assert.False((bool)isSafe.Invoke(null, ["//not-local.example"])!);
        Assert.True((bool)isSafe.Invoke(null, ["./child"])!);
        Assert.True((bool)isSafe.Invoke(null, ["../parent"])!);
        Assert.True((bool)isSafe.Invoke(null, ["http://example.test"])!);
        Assert.True((bool)isSafe.Invoke(null, ["https://example.test"])!);
        Assert.True((bool)isSafe.Invoke(null, ["mailto:hello@example.test"])!);
        Assert.False((bool)isSafe.Invoke(null, ["ftp://example.test"])!);
        Assert.False((bool)isSafe.Invoke(null, ["not a uri"])!);
        Assert.Equal("section", uniqueSlug.Invoke(null, ["!!!", used]));
        Assert.Equal("word", uniqueSlug.Invoke(null, ["  Word  ", used]));
        Assert.Equal("word-1", uniqueSlug.Invoke(null, ["word", used]));
        Assert.Equal(string.Empty, extractText.Invoke(null, [null]));

        var text = new StringBuilder();
        appendText.Invoke(null, [text, new LineBreakInline()]);
        Assert.Empty(text.ToString());

        var document = Markdig.Markdown.Parse("# A title");
        document.Descendants<HeadingBlock>().Single().GetAttributes().Id = "provided-id";
        var headings = Assert.IsAssignableFrom<IReadOnlyList<ZoorceMarkdownHeading>>(buildTableOfContents.Invoke(null, [document]));
        Assert.Equal("provided-id", headings.Single().Id);

        var generatedDocument = Markdig.Markdown.Parse("# A generated title");
        var generatedHeadings = Assert.IsAssignableFrom<IReadOnlyList<ZoorceMarkdownHeading>>(buildTableOfContents.Invoke(null, [generatedDocument]));
        Assert.Equal("a-generated-title", generatedHeadings.Single().Id);
    }

    [Fact]
    public void Markdown_tag_helper_owns_its_root_and_preserves_optional_class()
    {
        var helper = new ZoorceMarkdownTagHelper(GetRenderer()) { Markdown = "# Title", CssClass = "custom" };
        var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "markdown-test");
        var output = new TagHelperOutput("zoerce-markdown", new TagHelperAttributeList(), (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        helper.Process(context, output);

        Assert.Equal("article", output.TagName);
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Equal("z-markdown custom", output.Attributes["class"]!.Value);
        Assert.Contains("<h1", output.Content.GetContent());

        var withoutClass = new ZoorceMarkdownTagHelper(GetRenderer()) { Markdown = string.Empty };
        var noClassOutput = new TagHelperOutput("zoerce-markdown", new TagHelperAttributeList(), (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        withoutClass.Process(context, noClassOutput);
        Assert.Equal("z-markdown", noClassOutput.Attributes["class"]!.Value);
        Assert.Throws<ArgumentNullException>(() => helper.Process(null!, output));
        Assert.Throws<ArgumentNullException>(() => helper.Process(context, null!));
    }

    private static IZoorceMarkdownRenderer GetRenderer()
    {
        var services = new ServiceCollection();
        services.AddZoerceWebUI();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IZoorceMarkdownRenderer>();
    }

    private static string RenderHtml(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }
}
