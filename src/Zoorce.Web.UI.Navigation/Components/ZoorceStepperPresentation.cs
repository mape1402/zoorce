namespace Zoorce.Web.UI.Navigation.Stepper;

/// <summary>Defines the marker shown for a step when no state-specific icon takes precedence.</summary>
public enum ZoorceStepperMarkerStyle
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Number</c> option.</summary>
    Number,
    /// <summary>Represents the <c>Icon</c> option.</summary>
    Icon,
    /// <summary>Represents the <c>Check</c> option.</summary>
    Check,
    /// <summary>Represents the <c>Dot</c> option.</summary>
    Dot,
    /// <summary>Represents the <c>None</c> option.</summary>
    None
}

/// <summary>Defines how the current step is emphasized.</summary>
public enum ZoorceStepperActiveIndicator
{
    /// <summary>Represents the <c>Ring</c> option.</summary>
    Ring,
    /// <summary>Represents the <c>Halo</c> option.</summary>
    Halo,
    /// <summary>Represents the <c>Solid</c> option.</summary>
    Solid,
    /// <summary>Represents the <c>Outline</c> option.</summary>
    Outline
}

/// <summary>Defines optional motion applied to the current step.</summary>
public enum ZoorceStepperActiveMotion
{
    /// <summary>Represents the <c>None</c> option.</summary>
    None,
    /// <summary>Represents the <c>HaloPulse</c> option.</summary>
    HaloPulse,
    /// <summary>Represents the <c>RingPulse</c> option.</summary>
    RingPulse,
    /// <summary>Represents the <c>MarkerPulse</c> option.</summary>
    MarkerPulse,
    /// <summary>Represents the <c>ConnectorDraw</c> option.</summary>
    ConnectorDraw
}

/// <summary>Defines where labels are placed relative to a horizontal marker.</summary>
public enum ZoorceStepperLabelPlacement
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Below</c> option.</summary>
    Below,
    /// <summary>Represents the <c>Above</c> option.</summary>
    Above,
    /// <summary>Represents the <c>End</c> option.</summary>
    End
}

/// <summary>Defines the appearance of connectors between steps.</summary>
public enum ZoorceStepperConnectorStyle
{
    /// <summary>Represents the <c>Solid</c> option.</summary>
    Solid,
    /// <summary>Represents the <c>Dotted</c> option.</summary>
    Dotted,
    /// <summary>Represents the <c>Progress</c> option.</summary>
    Progress,
    /// <summary>Represents the <c>None</c> option.</summary>
    None
}

/// <summary>Defines a semantic presentation tone that can override the state-derived tone for a step.</summary>
public enum ZoorceStepperTone
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Neutral</c> option.</summary>
    Neutral,
    /// <summary>Represents the <c>Primary</c> option.</summary>
    Primary,
    /// <summary>Represents the <c>Success</c> option.</summary>
    Success,
    /// <summary>Represents the <c>Warning</c> option.</summary>
    Warning,
    /// <summary>Represents the <c>Danger</c> option.</summary>
    Danger,
    /// <summary>Represents the <c>Info</c> option.</summary>
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
