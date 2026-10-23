using Microsoft.AspNetCore.Html;
using Zoorce.Web.UI.Forms;

namespace Zoorce.Web.UI.Tests;

/// <summary>Verifies the independent contracts used by the custom select, combobox, and rating controls.</summary>
public sealed class SelectionControlContractTests
{
    [Fact]
    public void Select_preserves_clearability_and_rich_choice_template()
    {
        var template = (ZoorceFormChoice choice) => new HtmlString($"<strong>{choice.Label}</strong>");

        var model = new ZoorceSelectOptions
        {
            Id = "risk",
            Value = "standard",
            AllowClear = true,
            ChoiceTemplate = template,
            Choices = [new("standard", "Standard")]
        }.ToRenderOptions();

        Assert.Equal(ZoorceFormControlKind.Select, model.Kind);
        Assert.True(model.AllowClear);
        Assert.Same(template, model.ChoiceTemplate);
    }

    [Fact]
    public void Combobox_normalizes_its_opening_and_empty_result_contract()
    {
        var model = new ZoorceComboboxOptions
        {
            Id = "owner",
            OpenOnFocus = false,
            EmptyResultsText = "  No team available.  ",
            AllowCustomValues = true,
            Choices = [new("platform", "Platform")]
        }.ToRenderOptions();

        Assert.Equal(ZoorceFormControlKind.Combobox, model.Kind);
        Assert.False(model.OpenOnFocus);
        Assert.True(model.AllowCustomValues);
        Assert.Equal("No team available.", model.EmptyResultsText);
    }

    [Fact]
    public void Rating_preserves_cumulative_display_and_clear_contract()
    {
        var model = new ZoorceRatingOptions
        {
            Id = "confidence",
            AllowClear = true,
            DisplayMode = ZoorceRatingDisplayMode.Cumulative,
            Choices = [new("1", "One"), new("2", "Two")]
        }.ToRenderOptions();

        Assert.Equal(ZoorceFormControlKind.Rating, model.Kind);
        Assert.True(model.AllowClear);
        Assert.Equal(ZoorceRatingDisplayMode.Cumulative, model.RatingDisplayMode);
    }

    [Fact]
    public void Rating_rejects_an_unknown_display_mode()
    {
        var options = new ZoorceRatingOptions
        {
            Id = "confidence",
            DisplayMode = (ZoorceRatingDisplayMode)99
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.ToRenderOptions());
    }
}
