namespace Zoerce.WebUI.Components.Stepper;

/// <summary>Configures an isolated, progressively enhanced workflow stepper.</summary>
public sealed record ZoorceStepperOptions
{
    /// <summary>Gets the unique DOM identifier for the component.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the accessible label announced for the progress navigation.</summary>
    public string AriaLabel { get; init; } = "Workflow progress";

    /// <summary>Gets the steps rendered by the server before client-side enhancement.</summary>
    public IReadOnlyList<ZoorceStepperStep> Steps { get; init; } = [];

    /// <summary>Gets the identifier of the step currently in focus.</summary>
    public string? CurrentStepId { get; init; }

    /// <summary>Gets the visual treatment used for the stepper.</summary>
    public ZoorceStepperVariant Variant { get; init; } = ZoorceStepperVariant.Rail;

    /// <summary>Gets the preferred visual orientation.</summary>
    public ZoorceStepperOrientation Orientation { get; init; } = ZoorceStepperOrientation.Auto;

    /// <summary>Gets the behavior offered to visitors.</summary>
    public ZoorceStepperInteraction Interaction { get; init; } = ZoorceStepperInteraction.Progress;

    /// <summary>Gets the density used for labels, metadata, and gaps.</summary>
    public ZoorceStepperDensity Density { get; init; } = ZoorceStepperDensity.Comfortable;

    /// <summary>Gets whether connectors are rendered between adjacent steps.</summary>
    public bool ShowConnector { get; init; } = true;

    /// <summary>Gets whether a numeric fallback is rendered when no icon is supplied.</summary>
    public bool ShowNumbers { get; init; } = true;

    /// <summary>Gets whether state text and optional labels are visible.</summary>
    public bool ShowStatus { get; init; } = true;

    /// <summary>Gets the visual options shared by every step in this instance.</summary>
    public ZoorceStepperPresentation Presentation { get; init; } = new();

    /// <summary>Gets the declarative rules that control progress through the instance.</summary>
    public ZoorceStepperTransitionRules TransitionRules { get; init; } = new();

    /// <summary>Gets whether a visitor can revisit completed steps.</summary>
    public bool AllowCompletedNavigation { get; init; } = true;

    /// <summary>Gets whether direct navigation can skip to a future step.</summary>
    public bool AllowFutureNavigation { get; init; }

    /// <summary>Gets the text shown on the wizard previous control.</summary>
    public string PreviousLabel { get; init; } = "Back";

    /// <summary>Gets the text shown on the wizard next control.</summary>
    public string NextLabel { get; init; } = "Continue";

    /// <summary>Gets the text shown when the final wizard action is available.</summary>
    public string CompleteLabel { get; init; } = "Complete";

    /// <summary>Gets an optional same-origin endpoint that authorizes a requested transition.</summary>
    public string? TransitionUrl { get; init; }

    /// <summary>Gets an optional REST endpoint that returns the stepper snapshot.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Gets an optional SignalR hub endpoint used for live step state changes.</summary>
    public string? RealtimeHubUrl { get; init; }

    /// <summary>Gets the SignalR event name listened to by this stepper.</summary>
    public string? RealtimeEventName { get; init; }

    /// <summary>Gets the preferred transport used for the optional SignalR subscription.</summary>
    public ZoorceStepperRealtimeTransport RealtimeTransport { get; init; } = ZoorceStepperRealtimeTransport.Auto;

    /// <summary>Validates and normalizes host-supplied configuration.</summary>
    public ZoorceStepperOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentException("A stepper requires a unique id.", nameof(Id));
        }

        if (Steps.Count < 2)
        {
            throw new ArgumentException("A stepper requires at least two steps.", nameof(Steps));
        }

        var normalizedSteps = Steps.Select(step =>
        {
            ArgumentNullException.ThrowIfNull(step);
            if (string.IsNullOrWhiteSpace(step.Id))
            {
                throw new ArgumentException("Every step requires an id.", nameof(Steps));
            }

            return step with
            {
                Id = step.Id.Trim(),
                Title = string.IsNullOrWhiteSpace(step.Title) ? "Step" : step.Title.Trim(),
                SupportingText = TrimOrNull(step.SupportingText),
                Icon = TrimOrNull(step.Icon),
                StatusText = TrimOrNull(step.StatusText),
                TargetUrl = TrimOrNull(step.TargetUrl),
                PanelId = TrimOrNull(step.PanelId),
                Tone = Enum.IsDefined(step.Tone) ? step.Tone : ZoorceStepperTone.Auto
            };
        }).ToArray();

        if (normalizedSteps.Select(step => step.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedSteps.Length)
        {
            throw new ArgumentException("Step ids must be unique.", nameof(Steps));
        }

        var configuredCurrent = TrimOrNull(CurrentStepId);
        var current = configuredCurrent
            ?? normalizedSteps.FirstOrDefault(step => step.State == ZoorceStepperState.Active)?.Id
            ?? normalizedSteps.FirstOrDefault(step => step.State != ZoorceStepperState.Disabled)?.Id;

        if (current is null || !normalizedSteps.Any(step => string.Equals(step.Id, current, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The current step must identify one configured step.", nameof(CurrentStepId));
        }

        if (normalizedSteps.Any(step => string.Equals(step.Id, current, StringComparison.OrdinalIgnoreCase) && step.State == ZoorceStepperState.Disabled))
        {
            throw new ArgumentException("The current step cannot be disabled.", nameof(CurrentStepId));
        }

        var orientation = Variant switch
        {
            ZoorceStepperVariant.Segmented when Orientation == ZoorceStepperOrientation.Vertical => ZoorceStepperOrientation.Horizontal,
            ZoorceStepperVariant.FlatSegments when Orientation == ZoorceStepperOrientation.Vertical => ZoorceStepperOrientation.Horizontal,
            ZoorceStepperVariant.WizardHeader when Orientation == ZoorceStepperOrientation.Vertical => ZoorceStepperOrientation.Horizontal,
            ZoorceStepperVariant.Sidebar when Orientation == ZoorceStepperOrientation.Horizontal => ZoorceStepperOrientation.Vertical,
            _ => Orientation
        };

        return this with
        {
            Id = Id.Trim(),
            AriaLabel = string.IsNullOrWhiteSpace(AriaLabel) ? "Workflow progress" : AriaLabel.Trim(),
            Steps = normalizedSteps,
            CurrentStepId = current,
            Orientation = orientation,
            PreviousLabel = string.IsNullOrWhiteSpace(PreviousLabel) ? "Back" : PreviousLabel.Trim(),
            NextLabel = string.IsNullOrWhiteSpace(NextLabel) ? "Continue" : NextLabel.Trim(),
            CompleteLabel = string.IsNullOrWhiteSpace(CompleteLabel) ? "Complete" : CompleteLabel.Trim(),
            Presentation = NormalizePresentation(Presentation),
            TransitionRules = NormalizeTransitionRules(TransitionRules),
            TransitionUrl = TrimOrNull(TransitionUrl),
            SourceUrl = TrimOrNull(SourceUrl),
            RealtimeHubUrl = TrimOrNull(RealtimeHubUrl),
            RealtimeEventName = TrimOrNull(RealtimeEventName),
            RealtimeTransport = Enum.IsDefined(RealtimeTransport) ? RealtimeTransport : ZoorceStepperRealtimeTransport.Auto
        };
    }

    private static ZoorceStepperPresentation NormalizePresentation(ZoorceStepperPresentation? presentation)
    {
        var value = presentation ?? new ZoorceStepperPresentation();
        return value with
        {
            MarkerStyle = Enum.IsDefined(value.MarkerStyle) ? value.MarkerStyle : ZoorceStepperMarkerStyle.Auto,
            ActiveIndicator = Enum.IsDefined(value.ActiveIndicator) ? value.ActiveIndicator : ZoorceStepperActiveIndicator.Ring,
            ActiveMotion = Enum.IsDefined(value.ActiveMotion) ? value.ActiveMotion : ZoorceStepperActiveMotion.None,
            LabelPlacement = Enum.IsDefined(value.LabelPlacement) ? value.LabelPlacement : ZoorceStepperLabelPlacement.Auto,
            ConnectorStyle = Enum.IsDefined(value.ConnectorStyle) ? value.ConnectorStyle : ZoorceStepperConnectorStyle.Solid
        };
    }

    private static ZoorceStepperTransitionRules NormalizeTransitionRules(ZoorceStepperTransitionRules? rules)
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

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
