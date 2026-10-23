using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders an independently scoped mention editor.</summary>
public sealed class ZoorceMentionViewComponent : ViewComponent
{
    /// <summary>Renders the supplied mention configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMentionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        return View(options with { Id = control.Id, Name = control.Name, Label = control.Label, Description = control.Description, Value = control.Value, Placeholder = control.Placeholder, State = control.State, ValidationMessage = control.ValidationMessage, Autocomplete = control.Autocomplete, Metadata = control.Metadata, Choices = control.Choices, Triggers = string.IsNullOrWhiteSpace(options.Triggers) ? "@" : options.Triggers.Trim(), Source = string.IsNullOrWhiteSpace(options.Source) ? null : options.Source.Trim(), PlaceholderText = string.IsNullOrWhiteSpace(options.PlaceholderText) ? "Type @ to mention a teammate" : options.PlaceholderText.Trim() });
    }
}
