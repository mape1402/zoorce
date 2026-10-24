using Zoorce.Web.UI.Navigation.Stepper;

namespace Zoorce.Web.UI.Navigation.Wizard;

/// <summary>Describes one content-bearing step in a Zoorce wizard.</summary>
public sealed record ZoorceWizardStep(
    string Id,
    string Title,
    string? SupportingText = null,
    string? Icon = null,
    ZoorceStepperState State = ZoorceStepperState.Pending,
    string? StatusText = null,
    bool IsNavigable = true,
    bool IsOptional = false,
    bool? ShowSaveDraft = null,
    string? NextLabel = null,
    string? FinishLabel = null,
    string? FormId = null,
    ZoorceStepperTone Tone = ZoorceStepperTone.Auto);
