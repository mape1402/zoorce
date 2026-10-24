using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Forms;
using Zoorce.Web.UI.Navigation.Components;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies the contracts added for the reusable foundation, navigation, and advanced-selection controls.</summary>
public sealed class PendingControlsContractTests
{
    [Fact]
    public void Advanced_selection_fields_expose_distinct_render_kinds_and_normalize_defaults()
    {
        var choice = new ZoorceFormChoice(" team ", " Team ");

        Assert.Equal(ZoorceFormControlKind.AutoComplete, new ZoorceAutoCompleteOptions { Id = " auto ", Choices = [choice] }.ToRenderOptions().Kind);
        Assert.Equal(ZoorceFormControlKind.ListBox, new ZoorceListBoxOptions { Id = " list ", Choices = [choice] }.ToRenderOptions().Kind);
        Assert.Equal(ZoorceFormControlKind.MultiSelectDropdown, new ZoorceMultiSelectDropdownOptions { Id = " multi ", Choices = [choice] }.ToRenderOptions().Kind);
        Assert.Equal(ZoorceFormControlKind.DropdownTree, new ZoorceDropdownTreeOptions { Id = " tree " }.ToRenderOptions().Kind);
        Assert.Equal(ZoorceFormControlKind.Mention, new ZoorceMentionOptions { Id = " mention ", Choices = [choice] }.ToRenderOptions().Kind);
        Assert.Equal(ZoorceFormControlKind.MultiColumnCombobox, new ZoorceMultiColumnComboboxOptions { Id = " records " }.ToRenderOptions().Kind);

        var auto = new ZoorceAutoCompleteViewComponent().Invoke(new ZoorceAutoCompleteOptions { Id = " owner ", MinimumCharacters = -1, DebounceMilliseconds = 5_000 });
        var autoModel = Assert.IsType<ZoorceAutoCompleteOptions>(Assert.IsType<ViewViewComponentResult>(auto).ViewData!.Model);
        Assert.Equal("owner", autoModel.Id);
        Assert.Equal(0, autoModel.MinimumCharacters);
        Assert.Equal(2_000, autoModel.DebounceMilliseconds);

        var list = new ZoorceListBoxViewComponent().Invoke(new ZoorceListBoxOptions { Id = " lane ", MaxHeight = 1 });
        Assert.Equal(100, Assert.IsType<ZoorceListBoxOptions>(Assert.IsType<ViewViewComponentResult>(list).ViewData!.Model).MaxHeight);
    }

    [Fact]
    public void Multi_column_combobox_validates_columns_and_rows_before_rendering()
    {
        var component = new ZoorceMultiColumnComboboxViewComponent();
        var result = component.Invoke(new ZoorceMultiColumnComboboxOptions
        {
            Id = " service ",
            DisplayKey = " name ",
            Columns = [new ZoorceComboBoxColumn { Key = " name ", Header = " Name " }],
            Items = [new ZoorceMultiColumnChoice { Value = " api ", Values = new Dictionary<string, string?> { [" name "] = " API " } }]
        });

        var model = Assert.IsType<ZoorceMultiColumnComboboxOptions>(Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);
        Assert.Equal("service", model.Id);
        Assert.Equal("name", model.DisplayKey);
        Assert.Equal("name", model.Columns.Single().Key);
        Assert.Equal("api", model.Items.Single().Value);
        Assert.Equal("API", model.Items.Single().Values["name"]);

        Assert.Throws<ArgumentException>(() => component.Invoke(new ZoorceMultiColumnComboboxOptions
        {
            Id = "service",
            Columns = [new ZoorceComboBoxColumn { Key = "name", Header = "Name" }],
            DisplayKey = "unknown"
        }));
    }

    [Fact]
    public void Foundation_control_components_normalize_and_render_independently()
    {
        var avatar = new ZoorceAvatarViewComponent().Invoke(new ZoorceAvatarOptions { Label = "  Nora Ellis ", Size = 4, Status = " AWAY " });
        var badge = new ZoorceBadgeViewComponent().Invoke(new ZoorceBadgeOptions { Count = 101, MaximumCount = 99, Tone = " SUCCESS " });
        var card = new ZoorceCardViewComponent().Invoke(new ZoorceCardOptions { Id = " card ", Body = new HtmlString("content") });
        var dialog = new ZoorceDialogViewComponent().Invoke(new ZoorceDialogOptions
        {
            Id = " dialog ",
            Title = " Dialog ",
            Eyebrow = " ",
            HeaderMode = ZoorceDialogHeaderMode.None,
            BodyPadding = ZoorceDialogBodyPadding.Compact,
            CssClass = " dialog  contained ",
            Size = " unsupported ",
            FixedHeight = " 480px "
        });
        var dropdown = new ZoorceDropdownMenuViewComponent().Invoke(new ZoorceDropdownMenuOptions { Id = " menu ", Text = " More " });
        var fab = new ZoorceFloatingActionButtonViewComponent().Invoke(new ZoorceFloatingActionButtonOptions { Id = " add ", Label = " Add ", Icon = " + " });
        var message = new ZoorceMessageViewComponent().Invoke(new ZoorceMessageOptions { Id = " message ", Title = " Saved ", Tone = " SUCCESS " });
        var progress = new ZoorceProgressButtonViewComponent().Invoke(new ZoorceProgressButtonOptions { Id = " save ", Text = " Save ", Progress = 180 });
        var speedDial = new ZoorceSpeedDialViewComponent().Invoke(new ZoorceSpeedDialOptions { Id = " quick ", Label = " Quick " });

        Assert.Equal("NE", Assert.IsType<ZoorceAvatarOptions>(Assert.IsType<ViewViewComponentResult>(avatar).ViewData!.Model).Initials);
        Assert.Equal("99+", Assert.IsType<ZoorceBadgeOptions>(Assert.IsType<ViewViewComponentResult>(badge).ViewData!.Model).GetDisplayValue());
        Assert.Equal("card", Assert.IsType<ZoorceCardOptions>(Assert.IsType<ViewViewComponentResult>(card).ViewData!.Model).Id);
        var dialogModel = Assert.IsType<ZoorceDialogOptions>(Assert.IsType<ViewViewComponentResult>(dialog).ViewData!.Model);
        Assert.Equal("default", dialogModel.Size);
        Assert.Null(dialogModel.Eyebrow);
        Assert.Equal(ZoorceDialogHeaderMode.None, dialogModel.HeaderMode);
        Assert.Equal(ZoorceDialogBodyPadding.Compact, dialogModel.BodyPadding);
        Assert.Equal("dialog contained", dialogModel.CssClass);
        Assert.Equal("480px", dialogModel.FixedHeight);
        Assert.IsType<ViewViewComponentResult>(dropdown);
        Assert.Equal("+", Assert.IsType<ZoorceFloatingActionButtonOptions>(Assert.IsType<ViewViewComponentResult>(fab).ViewData!.Model).Icon);
        Assert.Equal("success", Assert.IsType<ZoorceMessageOptions>(Assert.IsType<ViewViewComponentResult>(message).ViewData!.Model).Tone);
        Assert.Equal(100, Assert.IsType<ZoorceProgressButtonOptions>(Assert.IsType<ViewViewComponentResult>(progress).ViewData!.Model).Progress);
        Assert.Equal("quick", Assert.IsType<ZoorceSpeedDialOptions>(Assert.IsType<ViewViewComponentResult>(speedDial).ViewData!.Model).Id);
        Assert.Throws<ArgumentException>(() => new ZoorceBadgeViewComponent().Invoke(new ZoorceBadgeOptions { Dot = true }));
        Assert.Throws<ArgumentException>(() => new ZoorceDialogViewComponent().Invoke(new ZoorceDialogOptions
        {
            Id = "dialog",
            Title = "Dialog",
            HeaderMode = ZoorceDialogHeaderMode.None,
            Header = new HtmlString("<span>Hidden</span>")
        }));
    }

    [Fact]
    public void Navigation_control_components_require_identity_and_render_their_boundaries()
    {
        var accordion = new ZoorceAccordionViewComponent().Invoke(new ZoorceAccordionOptions { Id = " accordion ", Items = [new ZoorceAccordionItem { Id = " one ", Title = " One " }] });
        var context = new ZoorceContextMenuViewComponent().Invoke(new ZoorceContextMenuOptions { Id = " context ", TargetSelector = " .target " });
        var menu = new ZoorceMenuBarViewComponent().Invoke(new ZoorceMenuBarOptions { Id = " menu " });
        var tabs = new ZoorceTabsViewComponent().Invoke(new ZoorceTabsOptions { Id = " tabs ", Items = [new ZoorceTabItem { Id = " first ", Text = " First ", Selected = true }] });
        var toolbar = new ZoorceToolbarViewComponent().Invoke(new ZoorceToolbarOptions { Id = " toolbar ", Label = " Actions " });
        var tree = new ZoorceTreeViewViewComponent().Invoke(new ZoorceTreeViewOptions { Id = " tree ", Label = " Areas ", Items = [new ZoorceTreeNode { Value = " foundation ", Text = " Foundation " }] });

        Assert.Equal("accordion", Assert.IsType<ZoorceAccordionOptions>(Assert.IsType<ViewViewComponentResult>(accordion).ViewData!.Model).Id);
        Assert.Equal("context", Assert.IsType<ZoorceContextMenuOptions>(Assert.IsType<ViewViewComponentResult>(context).ViewData!.Model).Id);
        Assert.Equal("menu", Assert.IsType<ZoorceMenuBarOptions>(Assert.IsType<ViewViewComponentResult>(menu).ViewData!.Model).Id);
        Assert.Equal("tabs", Assert.IsType<ZoorceTabsOptions>(Assert.IsType<ViewViewComponentResult>(tabs).ViewData!.Model).Id);
        Assert.Equal("Actions", Assert.IsType<ZoorceToolbarOptions>(Assert.IsType<ViewViewComponentResult>(toolbar).ViewData!.Model).Label);
        Assert.Equal("tree", Assert.IsType<ZoorceTreeViewOptions>(Assert.IsType<ViewViewComponentResult>(tree).ViewData!.Model).Id);
        Assert.Throws<ArgumentException>(() => new ZoorceTabsViewComponent().Invoke(new ZoorceTabsOptions { Id = " " }));
    }
}
