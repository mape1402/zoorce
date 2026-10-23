using Microsoft.AspNetCore.Mvc.ViewComponents;
using Zoorce.Web.UI.Foundation.Components;
using Zoorce.Web.UI.Foundation.Progress;
using Zoorce.Web.UI.Foundation.Skeleton;
using Zoorce.Web.UI.Foundation.WaitDialog;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies reusable asynchronous-feedback and interaction primitives.</summary>
public sealed class FoundationContractTests
{
    [Fact]
    public void Progress_skeleton_wait_button_and_tooltip_normalize_their_public_contracts()
    {
        var progress = new ZoorceProgressOptions { Id = " progress ", Kind = (ZoorceProgressKind)99, Value = 4, Label = " ", Tone = " SUCCESS ", Size = 1 }.Normalize();
        var skeleton = new ZoorceSkeletonOptions { Id = " skeleton ", Shape = (ZoorceSkeletonShape)99, Width = " ", Height = " 14px ", Count = 100, AriaLabel = " " }.Normalize();
        var wait = new ZoorceWaitDialogOptions { Id = " wait ", Title = " ", Description = " ", CancelLabel = " " }.Normalize();
        var button = new ZoorceButtonOptions { Text = " Save ", Tone = " ", Href = " /save ", Icon = " ✓ ", Id = " save ", AriaLabel = " Save draft " }.Normalize();
        var tooltip = new ZoorceTooltipOptions { Id = " hint ", Text = " Helpful text ", Placement = " diagonal " }.Normalize();

        Assert.Equal("progress", progress.Id);
        Assert.Equal(ZoorceProgressKind.Linear, progress.Kind);
        Assert.Equal(1, progress.Value);
        Assert.Equal("Loading", progress.Label);
        Assert.Equal("success", progress.Tone);
        Assert.Equal(16, progress.Size);
        Assert.Equal("skeleton", skeleton.Id);
        Assert.Equal(ZoorceSkeletonShape.Line, skeleton.Shape);
        Assert.Equal("100%", skeleton.Width);
        Assert.Equal("14px", skeleton.Height);
        Assert.Equal(64, skeleton.Count);
        Assert.Equal("Loading content", skeleton.AriaLabel);
        Assert.Equal("wait", wait.Id);
        Assert.Equal("Working", wait.Title);
        Assert.Null(wait.Description);
        Assert.Equal("Cancel", wait.CancelLabel);
        Assert.Equal("Save", button.Text);
        Assert.Equal("primary", button.Tone);
        Assert.Equal("/save", button.Href);
        Assert.Equal("✓", button.Icon);
        Assert.Equal("save", button.Id);
        Assert.Equal("Save draft", button.AriaLabel);
        Assert.Equal("hint", tooltip.Id);
        Assert.Equal("Helpful text", tooltip.Text);
        Assert.Equal("top", tooltip.Placement);
    }

    [Fact]
    public void Foundation_primitives_reject_missing_identity_and_render_normalized_models()
    {
        Assert.Throws<ArgumentException>(() => new ZoorceWaitDialogOptions().Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceButtonOptions { Text = " " }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceTooltipOptions { Id = " ", Text = "hint" }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceTooltipOptions { Id = "hint", Text = " " }.Normalize());

        var button = new ZoorceButtonViewComponent().Invoke(new ZoorceButtonOptions { Text = "Open" });
        var tooltip = new ZoorceTooltipViewComponent().Invoke(new ZoorceTooltipOptions { Id = "open-hint", Text = "Opens the draft" });
        Assert.IsType<ViewViewComponentResult>(button);
        Assert.IsType<ViewViewComponentResult>(tooltip);
    }
}
