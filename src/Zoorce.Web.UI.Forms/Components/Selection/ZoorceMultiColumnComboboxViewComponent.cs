using Microsoft.AspNetCore.Mvc;

namespace Zoorce.Web.UI.Forms;

/// <summary>Renders a multi-column selection dropdown.</summary>
public sealed class ZoorceMultiColumnComboboxViewComponent : ViewComponent
{
    /// <summary>Renders the supplied multi-column combobox configuration.</summary>
    public IViewComponentResult Invoke(ZoorceMultiColumnComboboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var control = options.ToRenderOptions();
        if (options.Columns.Count == 0) throw new ArgumentException("A multi-column combobox requires at least one column.", nameof(options));
        if (options.Columns.Any(column => column is null || string.IsNullOrWhiteSpace(column.Key) || string.IsNullOrWhiteSpace(column.Header))) throw new ArgumentException("Each multi-column combobox column requires a key and header.", nameof(options));
        var duplicateColumn = options.Columns.GroupBy(column => column.Key.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
        if (duplicateColumn is not null) throw new ArgumentException($"Column '{duplicateColumn.Key}' is duplicated.", nameof(options));
        if (options.Items.Any(item => item is null || string.IsNullOrWhiteSpace(item.Value))) throw new ArgumentException("Each multi-column combobox item requires a value.", nameof(options));
        var duplicateItem = options.Items.GroupBy(item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
        if (duplicateItem is not null) throw new ArgumentException($"Item '{duplicateItem.Key}' is duplicated.", nameof(options));

        var columns = options.Columns.Select(column => column with { Key = column.Key.Trim(), Header = column.Header.Trim() }).ToArray();
        var displayKey = string.IsNullOrWhiteSpace(options.DisplayKey) ? columns[0].Key : options.DisplayKey.Trim();
        if (!columns.Any(column => string.Equals(column.Key, displayKey, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("The display key must identify a configured column.", nameof(options));

        return View(options with
        {
            Id = control.Id,
            Name = control.Name,
            Label = control.Label,
            Description = control.Description,
            Value = control.Value,
            Placeholder = control.Placeholder,
            State = control.State,
            ValidationMessage = control.ValidationMessage,
            Autocomplete = control.Autocomplete,
            Metadata = control.Metadata,
            Source = string.IsNullOrWhiteSpace(options.Source) ? null : options.Source.Trim(),
            DisplayKey = displayKey,
            Columns = columns,
            Items = options.Items.Select(item => item with
            {
                Value = item.Value.Trim(),
                Values = item.Values.Where(pair => !string.IsNullOrWhiteSpace(pair.Key)).ToDictionary(pair => pair.Key.Trim(), pair => string.IsNullOrWhiteSpace(pair.Value) ? null : pair.Value.Trim(), StringComparer.OrdinalIgnoreCase)
            }).ToArray()
        });
    }
}
