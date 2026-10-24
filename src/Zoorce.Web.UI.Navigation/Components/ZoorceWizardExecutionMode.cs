namespace Zoorce.Web.UI.Navigation.Wizard;

/// <summary>Defines who authorizes a requested wizard action.</summary>
public enum ZoorceWizardExecutionMode
{
    /// <summary>Raises browser events and waits for the host to explicitly commit a transition.</summary>
    Manual,

    /// <summary>Posts each action to a same-origin host endpoint before changing wizard state.</summary>
    HostManaged
}
