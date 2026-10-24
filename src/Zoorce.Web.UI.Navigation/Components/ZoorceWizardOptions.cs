using Zoorce.Web.UI.Navigation.Stepper;

namespace Zoorce.Web.UI.Navigation.Wizard;

/// <summary>Configures a content-bearing, progressively enhanced wizard.</summary>
public sealed record ZoorceWizardOptions
{
    /// <summary>Gets the unique DOM identifier for the wizard.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the accessible label announced for wizard progress.</summary>
    public string AriaLabel { get; init; } = "Guided workflow";

    /// <summary>Gets the steps rendered by the wizard.</summary>
    public IReadOnlyList<ZoorceWizardStep> Steps { get; init; } = [];

    /// <summary>Gets the identifier of the step initially shown to the visitor.</summary>
    public string? CurrentStepId { get; init; }

    /// <summary>Gets the layout used for progress and content.</summary>
    public ZoorceWizardLayout Layout { get; init; } = ZoorceWizardLayout.Sidebar;

    /// <summary>Gets the active-panel context rendered below the progress rail.</summary>
    public ZoorceWizardPanelHeaderMode PanelHeaderMode { get; init; } = ZoorceWizardPanelHeaderMode.Full;

    /// <summary>Gets the spacing used around the progress rail.</summary>
    public ZoorceWizardSpacing ProgressSpacing { get; init; } = ZoorceWizardSpacing.Standard;

    /// <summary>Gets the spacing used around active panel content.</summary>
    public ZoorceWizardSpacing ContentSpacing { get; init; } = ZoorceWizardSpacing.Standard;

    /// <summary>Gets whether active content reserves a workspace height or uses its natural height.</summary>
    public ZoorceWizardContentHeight ContentHeight { get; init; } = ZoorceWizardContentHeight.Fill;

    /// <summary>Gets the spacing used around persistent footer actions.</summary>
    public ZoorceWizardSpacing FooterSpacing { get; init; } = ZoorceWizardSpacing.Standard;

    /// <summary>Gets the strategy that authorizes requested actions.</summary>
    public ZoorceWizardExecutionMode ExecutionMode { get; init; } = ZoorceWizardExecutionMode.Manual;

    /// <summary>Gets the declarative navigation rules used by the wizard.</summary>
    public ZoorceStepperTransitionRules TransitionRules { get; init; } = new();

    /// <summary>Gets whether completed steps can be revisited.</summary>
    public bool AllowCompletedNavigation { get; init; } = true;

    /// <summary>Gets whether direct navigation may skip to an eligible future step.</summary>
    public bool AllowFutureNavigation { get; init; }

    /// <summary>Gets whether the footer renders a cancel action.</summary>
    public bool ShowCancel { get; init; } = true;

    /// <summary>Gets whether the footer renders a save-draft action by default.</summary>
    public bool ShowSaveDraft { get; init; }

    /// <summary>Gets the label for the previous-step action.</summary>
    public string PreviousLabel { get; init; } = "Back";

    /// <summary>Gets the label for the next-step action.</summary>
    public string NextLabel { get; init; } = "Continue";

    /// <summary>Gets the label for the last-step action.</summary>
    public string FinishLabel { get; init; } = "Finish";

    /// <summary>Gets the label for the save-draft action.</summary>
    public string SaveDraftLabel { get; init; } = "Save draft";

    /// <summary>Gets the label for the cancel action.</summary>
    public string CancelLabel { get; init; } = "Cancel";

    /// <summary>Gets the optional same-origin endpoint used by host-managed wizards.</summary>
    public string? TransitionUrl { get; init; }

    /// <summary>Validates and normalizes host-supplied wizard configuration.</summary>
    public ZoorceWizardOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A wizard requires a unique id.", nameof(Id));
        }

        if (Steps.Count < 2)
        {
            throw new ArgumentException("A wizard requires at least two steps.", nameof(Steps));
        }

        var normalizedSteps = Steps.Select(step =>
        {
            ArgumentNullException.ThrowIfNull(step);
            if (string.IsNullOrWhiteSpace(step.Id))
            {
                throw new ArgumentException("Every wizard step requires an id.", nameof(Steps));
            }

            return step with
            {
                Id = step.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(step.Title) ? "Step" : step.Title.Trim(),
                SupportingText = TrimOrNull(step.SupportingText),
                Icon = TrimOrNull(step.Icon),
                StatusText = TrimOrNull(step.StatusText),
                NextLabel = TrimOrNull(step.NextLabel),
                FinishLabel = TrimOrNull(step.FinishLabel),
                FormId = TrimOrNull(step.FormId),
                Tone = Enum.IsDefined(step.Tone) ? step.Tone : ZoorceStepperTone.Auto
            };
        }).ToArray();

        if (normalizedSteps.Select(step => step.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedSteps.Length)
        {
            throw new ArgumentException("Wizard step ids must be unique.", nameof(Steps));
        }

        var current = TrimOrNull(CurrentStepId)
            ?? normalizedSteps.FirstOrDefault(step => step.State == ZoorceStepperState.Active)?.Id
            ?? normalizedSteps.FirstOrDefault(step => step.State != ZoorceStepperState.Disabled)?.Id;

        if (current is null || !normalizedSteps.Any(step => string.Equals(step.Id, current, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The current step must identify one configured step.", nameof(CurrentStepId));
        }

        if (normalizedSteps.Any(step => string.Equals(step.Id, current, StringComparison.OrdinalIgnoreCase) && step.State == ZoorceStepperState.Disabled))
        {
            throw new ArgumentException("The current wizard step cannot be disabled.", nameof(CurrentStepId));
        }

        if (ExecutionMode == ZoorceWizardExecutionMode.HostManaged && string.IsNullOrWhiteSpace(TransitionUrl))
        {
            throw new ArgumentException("Host-managed wizards require a transition endpoint.", nameof(TransitionUrl));
        }

        return this with
        {
            Id = Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Guided workflow" : AriaLabel.Trim(),
            Steps = normalizedSteps,
            CurrentStepId = current,
            Layout = Enum.IsDefined(Layout) ? Layout : ZoorceWizardLayout.Sidebar,
            PanelHeaderMode = Enum.IsDefined(PanelHeaderMode) ? PanelHeaderMode : ZoorceWizardPanelHeaderMode.Full,
            ProgressSpacing = Enum.IsDefined(ProgressSpacing) ? ProgressSpacing : ZoorceWizardSpacing.Standard,
            ContentSpacing = Enum.IsDefined(ContentSpacing) ? ContentSpacing : ZoorceWizardSpacing.Standard,
            ContentHeight = Enum.IsDefined(ContentHeight) ? ContentHeight : ZoorceWizardContentHeight.Fill,
            FooterSpacing = Enum.IsDefined(FooterSpacing) ? FooterSpacing : ZoorceWizardSpacing.Standard,
            ExecutionMode = Enum.IsDefined(ExecutionMode) ? ExecutionMode : ZoorceWizardExecutionMode.Manual,
            TransitionRules = NormalizeRules(TransitionRules),
            PreviousLabel = NormalizeLabel(PreviousLabel, "Back"),
            NextLabel = NormalizeLabel(NextLabel, "Continue"),
            FinishLabel = NormalizeLabel(FinishLabel, "Finish"),
            SaveDraftLabel = NormalizeLabel(SaveDraftLabel, "Save draft"),
            CancelLabel = NormalizeLabel(CancelLabel, "Cancel"),
            TransitionUrl = TrimOrNull(TransitionUrl)
        };
    }

    private static ZoorceStepperTransitionRules NormalizeRules(ZoorceStepperTransitionRules? rules)
    {
        var value = rules ?? new ZoorceStepperTransitionRules();
        return value with
        {
            Forward = Enum.IsDefined(value.Forward) ? value.Forward : ZoorceStepperForwardRule.Auto,
            Backward = Enum.IsDefined(value.Backward) ? value.Backward : ZoorceStepperBackwardRule.Auto,
            Warning = Enum.IsDefined(value.Warning) ? value.Warning : ZoorceStepperWarningRule.Allow,
            Error = Enum.IsDefined(value.Error) ? value.Error : ZoorceStepperErrorRule.Block,
            OnBackward = Enum.IsDefined(value.OnBackward) ? value.OnBackward : ZoorceStepperBackwardEffect.Auto
        };
    }

    private static string NormalizeLabel(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
