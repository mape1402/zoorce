namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a content-editable mention entry with trigger-aware suggestions.</summary>
public sealed record ZoorceMentionOptions : ZoorceChoiceFieldOptions
{
    /// <summary>Gets trigger characters that open the suggestion menu.</summary>
    public string Triggers { get; init; } = "@";

    /// <summary>Gets optional remote suggestion endpoint.</summary>
    public string? Source { get; init; }

    /// <summary>Gets placeholder copy shown while the editor is empty.</summary>
    public string PlaceholderText { get; init; } = "Type @ to mention a teammate";

    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.Mention };
}
