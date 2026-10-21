namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Describes one independently addressable step in a Zoorce stepper.</summary>
public sealed record ZoorceStepperStep(
    string Id,
    string Title,
    string? SupportingText = null,
    string? Icon = null,
    ZoorceStepperState State = ZoorceStepperState.Pending,
    string? StatusText = null,
    bool IsNavigable = true,
    bool IsOptional = false,
    string? TargetUrl = null,
    string? PanelId = null,
    ZoorceStepperTone Tone = ZoorceStepperTone.Auto);
