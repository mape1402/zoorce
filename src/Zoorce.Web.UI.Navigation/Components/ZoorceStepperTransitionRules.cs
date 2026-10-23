namespace Zoorce.Web.UI.Navigation.Stepper;

/// <summary>Defines how a visitor can move to a later step.</summary>
public enum ZoorceStepperForwardRule
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>AdjacentOnly</c> option.</summary>
    AdjacentOnly,
    /// <summary>Represents the <c>AnyEligible</c> option.</summary>
    AnyEligible,
    /// <summary>Represents the <c>Block</c> option.</summary>
    Block
}

/// <summary>Defines how a visitor can revisit an earlier step.</summary>
public enum ZoorceStepperBackwardRule
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>AnyEligible</c> option.</summary>
    AnyEligible,
    /// <summary>Represents the <c>CompletedOnly</c> option.</summary>
    CompletedOnly,
    /// <summary>Represents the <c>Block</c> option.</summary>
    Block
}

/// <summary>Defines how a warning state affects forward progress.</summary>
public enum ZoorceStepperWarningRule
{
    /// <summary>Represents the <c>Allow</c> option.</summary>
    Allow,
    /// <summary>Represents the <c>RequireAuthorization</c> option.</summary>
    RequireAuthorization,
    /// <summary>Represents the <c>Block</c> option.</summary>
    Block
}

/// <summary>Defines how an error state affects forward progress.</summary>
public enum ZoorceStepperErrorRule
{
    /// <summary>Represents the <c>Block</c> option.</summary>
    Block,
    /// <summary>Represents the <c>RequireAuthorization</c> option.</summary>
    RequireAuthorization,
    /// <summary>Represents the <c>Allow</c> option.</summary>
    Allow
}

/// <summary>Defines how later steps are treated after a visitor moves backward.</summary>
public enum ZoorceStepperBackwardEffect
{
    /// <summary>Represents the <c>Auto</c> option.</summary>
    Auto,
    /// <summary>Represents the <c>Preserve</c> option.</summary>
    Preserve,
    /// <summary>Represents the <c>ResetFollowing</c> option.</summary>
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
