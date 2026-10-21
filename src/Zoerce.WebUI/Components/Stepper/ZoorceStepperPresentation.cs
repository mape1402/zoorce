namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines the marker shown for a step when no state-specific icon takes precedence.</summary>
public enum ZoorceStepperMarkerStyle
{
    Auto,
    Number,
    Icon,
    Check,
    Dot,
    None
}

/// <summary>Defines how the current step is emphasized.</summary>
public enum ZoorceStepperActiveIndicator
{
    Ring,
    Halo,
    Solid,
    Outline
}

/// <summary>Defines optional motion applied to the current step.</summary>
public enum ZoorceStepperActiveMotion
{
    None,
    HaloPulse,
    RingPulse,
    MarkerPulse,
    ConnectorDraw
}

/// <summary>Defines where labels are placed relative to a horizontal marker.</summary>
public enum ZoorceStepperLabelPlacement
{
    Auto,
    Below,
    Above,
    End
}

/// <summary>Defines the appearance of connectors between steps.</summary>
public enum ZoorceStepperConnectorStyle
{
    Solid,
    Dotted,
    Progress,
    None
}

/// <summary>Defines a semantic presentation tone that can override the state-derived tone for a step.</summary>
public enum ZoorceStepperTone
{
    Auto,
    Neutral,
    Primary,
    Success,
    Warning,
    Danger,
    Info
}

/// <summary>Groups display choices that are shared by every step in one stepper instance.</summary>
public sealed record ZoorceStepperPresentation
{
    /// <summary>Gets the marker appearance used by the instance.</summary>
    public ZoorceStepperMarkerStyle MarkerStyle { get; init; } = ZoorceStepperMarkerStyle.Auto;

    /// <summary>Gets the visual emphasis used by the current marker.</summary>
    public ZoorceStepperActiveIndicator ActiveIndicator { get; init; } = ZoorceStepperActiveIndicator.Ring;

    /// <summary>Gets the optional motion used by the current marker.</summary>
    public ZoorceStepperActiveMotion ActiveMotion { get; init; } = ZoorceStepperActiveMotion.None;

    /// <summary>Gets the preferred label placement.</summary>
    public ZoorceStepperLabelPlacement LabelPlacement { get; init; } = ZoorceStepperLabelPlacement.Auto;

    /// <summary>Gets the visual style used for adjacent connectors.</summary>
    public ZoorceStepperConnectorStyle ConnectorStyle { get; init; } = ZoorceStepperConnectorStyle.Solid;
}
