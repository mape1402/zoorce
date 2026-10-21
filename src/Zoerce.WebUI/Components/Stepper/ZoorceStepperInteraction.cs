namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines how visitors can interact with a stepper.</summary>
public enum ZoorceStepperInteraction
{
    /// <summary>Presents progress only and does not expose step navigation controls.</summary>
    Progress,

    /// <summary>Exposes previous and next controls for a linear guided flow.</summary>
    Wizard,

    /// <summary>Lets the host authorize direct navigation to individual steps.</summary>
    Navigation
}
