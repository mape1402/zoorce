namespace Zoerce.WebUI.Components.DataGrid;

/// <summary>Describes one safe text column rendered by <see cref="ZoorceDataGridViewComponent"/>.</summary>
public sealed record ZoorceDataGridColumn(string Field, string Title, bool Sortable = true);
