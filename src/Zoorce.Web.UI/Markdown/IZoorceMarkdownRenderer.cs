namespace Zoorce.Web.UI.Markdown;

/// <summary>Renders untrusted Markdown through Zoorce's safe rendering policy.</summary>
public interface IZoorceMarkdownRenderer
{
    /// <summary>Renders Markdown into trusted HTML and a generated table of contents.</summary>
    ZoorceMarkdownRenderResult Render(string markdown);
}
