namespace Zoorce.Web.UI.Forms;
/// <summary>Configures a duration input.</summary>
public sealed record ZoorceDurationInputOptions : ZoorceFieldOptions
{
    /// <summary>Gets the hours component.</summary>
    public string? Hours { get; init; }
    /// <summary>Gets the minutes component.</summary>
    public string? Minutes { get; init; }
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions() => base.CreateRenderOptions() with { Kind = ZoorceFormControlKind.Duration, Value = Hours ?? Value, SecondaryValue = Minutes, Metadata = Metadata };
}
