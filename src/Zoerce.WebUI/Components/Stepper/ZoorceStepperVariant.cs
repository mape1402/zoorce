namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines the visual presentation used by a stepper.</summary>
public enum ZoorceStepperVariant
{
    /// <summary>Renders numbered or icon-based markers connected by a progress rail.</summary>
    Rail,

    /// <summary>Renders compact chevron-like segments for short horizontal flows.</summary>
    Segmented,

    /// <summary>Renders a dense vertical timeline suited to a companion content panel.</summary>
    Sidebar,

    /// <summary>Renders expressive markers, supporting text, and status badges.</summary>
    Workflow
}
