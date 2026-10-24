namespace Zoorce.Web.UI.Navigation.Wizard;

/// <summary>Defines how much of the active step context is rendered above a wizard panel.</summary>
public enum ZoorceWizardPanelHeaderMode
{
    /// <summary>Shows the step count, title, and supporting text.</summary>
    Full,

    /// <summary>Shows only the step title.</summary>
    Title,

    /// <summary>Hides duplicate visual context while preserving an accessible panel name.</summary>
    Hidden
}
