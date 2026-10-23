namespace Zoorce.Web.UI.Markdown;

/// <summary>Represents one navigable heading generated from Markdown.</summary>
public sealed record ZoorceMarkdownHeading(string Id, string Text, int Level);
