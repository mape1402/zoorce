namespace Zoorce.Web.UI.Forms;

internal sealed record ZoorceDropdownTreeNodeModel(
    ZoorceTreeChoice Item,
    string? Value,
    bool Multiple,
    bool Expanded,
    bool Disabled,
    int Level);
