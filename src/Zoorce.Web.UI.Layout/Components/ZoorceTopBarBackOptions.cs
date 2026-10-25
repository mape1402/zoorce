namespace Zoorce.Web.UI.Layout.Components;

/// <summary>Configures the host-owned back action in the top bar.</summary>
public sealed class ZoorceTopBarBackOptions
{
    /// <summary>Gets or sets whether the action is visible. It is hidden by default.</summary>
    public bool Visible { get; set; }

    /// <summary>Gets or sets the accessible label and tooltip.</summary>
    public string Label { get; set; } = "Back";

    /// <summary>Gets or sets a navigation destination. If omitted, the host handles the button click.</summary>
    public string? Url { get; set; }
}
