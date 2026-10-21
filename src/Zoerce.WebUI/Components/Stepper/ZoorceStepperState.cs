namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines the lifecycle state displayed by an individual step.</summary>
public enum ZoorceStepperState
{
    /// <summary>The step has not started.</summary>
    Pending,

    /// <summary>The step is currently being worked on.</summary>
    Active,

    /// <summary>The step has completed successfully.</summary>
    Completed,

    /// <summary>The step needs a non-blocking review or extra information.</summary>
    Attention,

    /// <summary>The step cannot proceed until an error is resolved.</summary>
    Error,

    /// <summary>The step is currently unavailable.</summary>
    Disabled
}
