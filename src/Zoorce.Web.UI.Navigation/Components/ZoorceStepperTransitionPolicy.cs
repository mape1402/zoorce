namespace Zoorce.Web.UI.Navigation.Stepper;

/// <summary>Describes a transition requested by an isolated stepper instance.</summary>
public sealed record ZoorceStepperTransitionRequest(
    string StepperId,
    string StepId,
    string PreviousStepId,
    string Reason,
    bool IsBackward);

/// <summary>Describes the server authorization result for a requested step transition.</summary>
public sealed record ZoorceStepperTransitionDecision(
    bool Allowed,
    string? Message = null,
    ZoorceStepperSnapshot? Snapshot = null);

/// <summary>Provides the current server-authorized snapshot for a stepper instance.</summary>
public sealed record ZoorceStepperSnapshot(
    string? CurrentStepId,
    IReadOnlyList<ZoorceStepperStepSnapshot> Steps);

/// <summary>Describes a mutable step state sent to an isolated client instance.</summary>
public sealed record ZoorceStepperStepSnapshot(
    string Id,
    ZoorceStepperState? State = null,
    ZoorceStepperTone? Tone = null,
    string? Title = null,
    string? SupportingText = null,
    string? StatusText = null,
    string? Icon = null,
    bool? IsNavigable = null,
    string? TargetUrl = null);

/// <summary>Authorizes transitions in the host's business domain before the browser updates its local state.</summary>
public interface IZoorceStepperTransitionPolicy
{
    /// <summary>Evaluates a requested transition.</summary>
    ValueTask<ZoorceStepperTransitionDecision> EvaluateAsync(ZoorceStepperTransitionRequest request, CancellationToken cancellationToken = default);
}
