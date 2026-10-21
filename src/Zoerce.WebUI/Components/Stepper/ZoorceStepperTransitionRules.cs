namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Defines how a visitor can move to a later step.</summary>
public enum ZoorceStepperForwardRule
{
    Auto,
    AdjacentOnly,
    AnyEligible,
    Block
}

/// <summary>Defines how a visitor can revisit an earlier step.</summary>
public enum ZoorceStepperBackwardRule
{
    Auto,
    AnyEligible,
    CompletedOnly,
    Block
}

/// <summary>Defines how a warning state affects forward progress.</summary>
public enum ZoorceStepperWarningRule
{
    Allow,
    RequireAuthorization,
    Block
}

/// <summary>Defines how an error state affects forward progress.</summary>
public enum ZoorceStepperErrorRule
{
    Block,
    RequireAuthorization,
    Allow
}

/// <summary>Defines how later steps are treated after a visitor moves backward.</summary>
public enum ZoorceStepperBackwardEffect
{
    Auto,
    Preserve,
    ResetFollowing
}

/// <summary>Groups the declarative navigation rules for one stepper instance.</summary>
public sealed record ZoorceStepperTransitionRules
{
    /// <summary>Gets the forward-navigation rule.</summary>
    public ZoorceStepperForwardRule Forward { get; init; } = ZoorceStepperForwardRule.Auto;

    /// <summary>Gets the backward-navigation rule.</summary>
    public ZoorceStepperBackwardRule Backward { get; init; } = ZoorceStepperBackwardRule.Auto;

    /// <summary>Gets the rule used when leaving a warning step.</summary>
    public ZoorceStepperWarningRule Warning { get; init; } = ZoorceStepperWarningRule.Allow;

    /// <summary>Gets the rule used when leaving an error step.</summary>
    public ZoorceStepperErrorRule Error { get; init; } = ZoorceStepperErrorRule.Block;

    /// <summary>Gets the effect applied to later steps after a backward move.</summary>
    public ZoorceStepperBackwardEffect OnBackward { get; init; } = ZoorceStepperBackwardEffect.Auto;
}
