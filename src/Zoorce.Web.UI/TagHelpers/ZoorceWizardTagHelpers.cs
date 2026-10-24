using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Zoorce.Web.UI.Navigation.Stepper;
using Zoorce.Web.UI.Navigation.Wizard;

namespace Zoorce.Web.UI.TagHelpers;

/// <summary>Renders a content-bearing wizard with progress navigation and a persistent action footer.</summary>
[HtmlTargetElement("zoorce-wizard")]
public sealed class ZoorceWizardTagHelper : TagHelper
{
    internal static readonly object PanelCollectionKey = new();

    /// <summary>Gets or sets the wizard configuration.</summary>
    [HtmlAttributeName("options")]
    public ZoorceWizardOptions Options { get; set; } = new();

    /// <summary>Gets or sets additional CSS classes added to the wizard root.</summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var panels = new ZoorceWizardPanelCollection();
        context.Items[PanelCollectionKey] = panels;
        await output.GetChildContentAsync();

        var options = Options.Normalize();
        ValidatePanels(options, panels);
        var currentIndex = options.Steps.ToList().FindIndex(step => string.Equals(step.Id, options.CurrentStepId, StringComparison.OrdinalIgnoreCase));
        var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        var configuration = JsonSerializer.Serialize(new
        {
            currentStepId = options.CurrentStepId,
            layout = options.Layout,
            executionMode = options.ExecutionMode,
            allowCompletedNavigation = options.AllowCompletedNavigation,
            allowFutureNavigation = options.AllowFutureNavigation,
            transitionRules = options.TransitionRules,
            transitionUrl = options.TransitionUrl,
            showSaveDraft = options.ShowSaveDraft
        }, serializerOptions);

        output.TagName = "section";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("options");
        output.Attributes.RemoveAll("css-class");
        output.Attributes.SetAttribute("id", options.Id);
        output.Attributes.SetAttribute("class", $"z-component z-wizard z-wizard--{options.Layout.ToString().ToLowerInvariant()} z-wizard--panel-header-{options.PanelHeaderMode.ToString().ToLowerInvariant()} z-wizard--progress-spacing-{options.ProgressSpacing.ToString().ToLowerInvariant()} z-wizard--content-spacing-{options.ContentSpacing.ToString().ToLowerInvariant()} z-wizard--content-height-{options.ContentHeight.ToString().ToLowerInvariant()} z-wizard--footer-spacing-{options.FooterSpacing.ToString().ToLowerInvariant()} {CssClass}".Trim());
        output.Attributes.SetAttribute("data-z-wizard", string.Empty);
        output.Attributes.SetAttribute("data-z-wizard-layout", options.Layout);
        output.Attributes.SetAttribute("data-z-wizard-current", options.CurrentStepId);
        output.Attributes.SetAttribute("style", $"--z-wizard-step-count:{options.Steps.Count}");
        output.Attributes.SetAttribute("aria-label", options.AriaLabel);

        var content = new HtmlContentBuilder();
        content.AppendHtml("<script type=\"application/json\" data-z-wizard-config>");
        content.AppendHtml(configuration);
        content.AppendHtml("</script>");
        RenderProgress(content, options, currentIndex);
        RenderPanels(content, options, panels, currentIndex);
        RenderFooter(content, options, currentIndex);
        content.AppendHtml("<p class=\"z-visually-hidden\" data-z-wizard-live aria-live=\"polite\"></p>");
        content.AppendHtml($"<noscript><p class=\"z-wizard-noscript\">{Encode(options.AriaLabel)}: {Encode(options.Steps[currentIndex].Title)}</p></noscript>");
        output.Content.SetHtmlContent(content);
    }

    private static void ValidatePanels(ZoorceWizardOptions options, ZoorceWizardPanelCollection panels)
    {
        if (panels.Panels.Count != options.Steps.Count)
        {
            throw new ArgumentException("A wizard panel is required for every configured step.", nameof(Options));
        }

        var stepIds = options.Steps.Select(step => step.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownPanel = panels.Panels.Keys.FirstOrDefault(id => !stepIds.Contains(id));
        if (unknownPanel is not null)
        {
            throw new ArgumentException($"Wizard panel '{unknownPanel}' does not identify a configured step.", nameof(Options));
        }

        var missingPanel = options.Steps.FirstOrDefault(step => !panels.Panels.ContainsKey(step.Id));
        if (missingPanel is not null)
        {
            throw new ArgumentException($"Wizard step '{missingPanel.Id}' does not have a panel.", nameof(Options));
        }
    }

    private static void RenderProgress(HtmlContentBuilder content, ZoorceWizardOptions options, int currentIndex)
    {
        content.AppendHtml($"<nav class=\"z-wizard-progress\" aria-label=\"{Encode(options.AriaLabel)}\"><ol class=\"z-wizard-step-list\">");
        for (var index = 0; index < options.Steps.Count; index++)
        {
            var step = options.Steps[index];
            var current = index == currentIndex;
            var disabled = step.State == ZoorceStepperState.Disabled || !step.IsNavigable;
            var panelId = PanelDomId(options.Id, step.Id);
            var classes = $"z-wizard-step is-{step.State.ToString().ToLowerInvariant()}" + (current ? " is-current" : string.Empty) + (step.Tone != ZoorceStepperTone.Auto ? $" has-tone tone-{step.Tone.ToString().ToLowerInvariant()}" : string.Empty);
            var marker = step.State == ZoorceStepperState.Completed ? "✓" : !string.IsNullOrWhiteSpace(step.Icon) ? step.Icon : (index + 1).ToString();
            var status = step.IsOptional ? "Optional" : step.StatusText ?? StateText(step.State);

            var saveDraftPreference = step.ShowSaveDraft is bool showSaveDraft ? showSaveDraft.ToString().ToLowerInvariant() : string.Empty;
            var currentAttribute = current ? " aria-current=\"step\"" : string.Empty;
            content.AppendHtml($"<li class=\"{classes}\" data-z-wizard-step data-z-step-id=\"{Encode(step.Id)}\" data-z-step-state=\"{step.State}\" data-z-step-tone=\"{step.Tone}\" data-z-step-index=\"{index}\" data-z-step-navigable=\"{step.IsNavigable.ToString().ToLowerInvariant()}\" data-z-step-optional=\"{step.IsOptional.ToString().ToLowerInvariant()}\" data-z-step-show-save-draft=\"{saveDraftPreference}\" data-z-step-next-label=\"{Encode(step.NextLabel)}\" data-z-step-finish-label=\"{Encode(step.FinishLabel)}\" data-z-step-form-id=\"{Encode(step.FormId)}\">");
            content.AppendHtml($"<button type=\"button\" class=\"z-wizard-step-trigger\" data-z-wizard-step-trigger aria-label=\"{Encode(step.Title)}\" aria-controls=\"{Encode(panelId)}\"{currentAttribute} aria-disabled=\"{disabled.ToString().ToLowerInvariant()}\" tabindex=\"{(current ? "0" : "-1")}\"{(disabled ? " disabled" : string.Empty)}>");
            content.AppendHtml($"<span class=\"z-wizard-step-marker\" data-z-wizard-step-marker data-z-wizard-marker-default=\"{Encode(marker)}\" aria-hidden=\"true\">{Encode(marker)}</span>");
            content.AppendHtml("<span class=\"z-wizard-step-copy\">");
            content.AppendHtml($"<strong data-z-wizard-step-title>{Encode(step.Title)}</strong>");
            if (!string.IsNullOrWhiteSpace(step.SupportingText)) content.AppendHtml($"<small data-z-wizard-step-supporting>{Encode(step.SupportingText)}</small>");
            content.AppendHtml($"<em data-z-wizard-step-status>{Encode(status)}</em>");
            content.AppendHtml("</span></button></li>");
        }

        content.AppendHtml("</ol></nav>");
    }

    private static void RenderPanels(HtmlContentBuilder content, ZoorceWizardOptions options, ZoorceWizardPanelCollection panels, int currentIndex)
    {
        content.AppendHtml("<div class=\"z-wizard-content\" data-z-wizard-content>");
        for (var index = 0; index < options.Steps.Count; index++)
        {
            var step = options.Steps[index];
            var panelId = PanelDomId(options.Id, step.Id);
            var headingId = $"{panelId}-title";
            var hidden = index == currentIndex ? string.Empty : " hidden";
            content.AppendHtml($"<section class=\"z-wizard-panel\" id=\"{Encode(panelId)}\" data-z-wizard-panel data-z-step-id=\"{Encode(step.Id)}\" aria-labelledby=\"{Encode(headingId)}\" tabindex=\"-1\"{hidden}>");
            RenderPanelHeader(content, options, step, index, headingId);
            content.AppendHtml("<div class=\"z-wizard-panel-body\">");
            content.AppendHtml(panels.Panels[step.Id]);
            content.AppendHtml("</div><div class=\"z-wizard-field-errors\" data-z-wizard-field-errors aria-live=\"polite\"></div></section>");
        }

        content.AppendHtml("</div>");
    }

    private static void RenderPanelHeader(HtmlContentBuilder content, ZoorceWizardOptions options, ZoorceWizardStep step, int index, string headingId)
    {
        if (options.PanelHeaderMode == ZoorceWizardPanelHeaderMode.Hidden)
        {
            content.AppendHtml($"<h2 id=\"{Encode(headingId)}\" class=\"z-visually-hidden\">{Encode(step.Title)}</h2>");
            return;
        }

        content.AppendHtml("<header class=\"z-wizard-panel-header\">");
        if (options.PanelHeaderMode == ZoorceWizardPanelHeaderMode.Full)
        {
            content.AppendHtml($"<span>Step {index + 1} of {options.Steps.Count}</span>");
        }
        content.AppendHtml($"<h2 id=\"{Encode(headingId)}\" tabindex=\"-1\" data-z-wizard-panel-focus>{Encode(step.Title)}</h2>");
        if (options.PanelHeaderMode == ZoorceWizardPanelHeaderMode.Full && !string.IsNullOrWhiteSpace(step.SupportingText))
        {
            content.AppendHtml($"<p>{Encode(step.SupportingText)}</p>");
        }
        content.AppendHtml("</header>");
    }

    private static void RenderFooter(HtmlContentBuilder content, ZoorceWizardOptions options, int currentIndex)
    {
        var step = options.Steps[currentIndex];
        var showSave = options.ShowSaveDraft || options.Steps.Any(candidate => candidate.ShowSaveDraft == true);
        var nextLabel = currentIndex == options.Steps.Count - 1
            ? step.FinishLabel ?? options.FinishLabel
            : step.NextLabel ?? options.NextLabel;
        content.AppendHtml("<footer class=\"z-wizard-footer\" data-z-wizard-footer>");
        content.AppendHtml("<div class=\"z-wizard-footer-secondary\">");
        if (options.ShowCancel) content.AppendHtml($"<button type=\"button\" class=\"z-button z-button-ghost\" data-z-wizard-cancel>{Encode(options.CancelLabel)}</button>");
        if (showSave) content.AppendHtml($"<button type=\"button\" class=\"z-button z-button-secondary\" data-z-wizard-save>{Encode(options.SaveDraftLabel)}</button>");
        content.AppendHtml("</div><div class=\"z-wizard-footer-primary\">");
        content.AppendHtml($"<button type=\"button\" class=\"z-button z-button-ghost\" data-z-wizard-previous{(currentIndex == 0 ? " disabled" : string.Empty)}>{Encode(options.PreviousLabel)}</button>");
        content.AppendHtml($"<button type=\"button\" class=\"z-button z-button-primary\" data-z-wizard-next data-z-wizard-next-label=\"{Encode(options.NextLabel)}\" data-z-wizard-finish-label=\"{Encode(options.FinishLabel)}\">{Encode(nextLabel)}</button>");
        content.AppendHtml("</div></footer>");
    }

    private static string StateText(ZoorceStepperState state) => state switch
    {
        ZoorceStepperState.Active => "In progress",
        ZoorceStepperState.Completed => "Completed",
        ZoorceStepperState.Attention => "Needs review",
        ZoorceStepperState.Error => "Action required",
        ZoorceStepperState.Disabled => "Unavailable",
        _ => "Pending"
    };

    private static string PanelDomId(string wizardId, string stepId) => $"{wizardId}-{stepId}-panel";

    private static string Encode(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);
}

/// <summary>Captures arbitrary Razor content for one configured wizard step.</summary>
[HtmlTargetElement("zoorce-wizard-panel", ParentTag = "zoorce-wizard")]
public sealed class ZoorceWizardPanelTagHelper : TagHelper
{
    /// <summary>Gets or sets the id of the step that owns this panel.</summary>
    [HtmlAttributeName("step-id")]
    public string StepId { get; set; } = string.Empty;

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (!context.Items.TryGetValue(ZoorceWizardTagHelper.PanelCollectionKey, out var value) || value is not ZoorceWizardPanelCollection panels)
        {
            throw new InvalidOperationException("A wizard panel must be nested inside a zoorce-wizard element.");
        }

        if (string.IsNullOrWhiteSpace(StepId))
        {
            throw new ArgumentException("A wizard panel requires a step-id.", nameof(StepId));
        }

        var normalizedId = StepId.Trim();
        if (!panels.Panels.TryAdd(normalizedId, await output.GetChildContentAsync()))
        {
            throw new ArgumentException($"Wizard panel '{normalizedId}' was declared more than once.", nameof(StepId));
        }

        output.SuppressOutput();
    }
}

internal sealed class ZoorceWizardPanelCollection
{
    public Dictionary<string, IHtmlContent> Panels { get; } = new(StringComparer.OrdinalIgnoreCase);
}
