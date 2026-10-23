namespace Zoorce.Web.UI.Foundation.Components;

/// <summary>Configures a persistent inline message or alert.</summary>
public sealed record ZoorceMessageOptions
{
    /// <summary>Gets the unique DOM identifier for the message.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the message title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets optional descriptive copy.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the semantic visual tone.</summary>
    public string Tone { get; init; } = "info";

    /// <summary>Gets whether the message can be dismissed by its user.</summary>
    public bool Dismissible { get; init; }

    /// <summary>Gets optional action text.</summary>
    public string? ActionText { get; init; }
}
