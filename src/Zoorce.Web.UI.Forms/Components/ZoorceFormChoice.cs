namespace Zoorce.Web.UI.Forms;

/// <summary>Describes an available value for a choice-based form control.</summary>
public sealed record ZoorceFormChoice(
    string Value,
    string Label,
    string? Description = null,
    string? Icon = null,
    bool Disabled = false,
    string? Group = null);
