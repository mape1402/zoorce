using Zoorce.Web.UI.Navigation.Stepper;

namespace Zoorce.Web.UI.Navigation.Wizard;

/// <summary>Describes an action requested by an isolated wizard instance.</summary>
public sealed record ZoorceWizardTransitionRequest(
    string WizardId,
    string Action,
    string CurrentStepId,
    string? TargetStepId = null,
    bool IsBackward = false,
    bool RequiresAuthorization = false,
    IReadOnlyDictionary<string, string[]>? Values = null);

/// <summary>Describes a server result authorizing or rejecting a wizard action.</summary>
public sealed record ZoorceWizardTransitionDecision(
    bool Allowed,
    string? Message = null,
    ZoorceWizardSnapshot? Snapshot = null,
    IReadOnlyDictionary<string, string[]>? FieldErrors = null);

/// <summary>Provides the current host-authorized state for one wizard instance.</summary>
public sealed record ZoorceWizardSnapshot(
    string? CurrentStepId,
    IReadOnlyList<ZoorceWizardStepSnapshot> Steps);

/// <summary>Describes a step update applied to an isolated wizard instance.</summary>
public sealed record ZoorceWizardStepSnapshot(
    string Id,
    ZoorceStepperState? State = null,
    ZoorceStepperTone? Tone = null,
    string? Title = null,
    string? SupportingText = null,
    string? StatusText = null,
    string? Icon = null,
    bool? IsNavigable = null);

/// <summary>Authorizes host-managed wizard actions in the host's business domain.</summary>
public interface IZoorceWizardTransitionPolicy
{
    /// <summary>Evaluates a wizard action before the browser changes its local state.</summary>
    ValueTask<ZoorceWizardTransitionDecision> EvaluateAsync(ZoorceWizardTransitionRequest request, CancellationToken cancellationToken = default);
}
