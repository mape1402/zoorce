using Zoorce.Web.UI.Forms;
using Zoorce.Web.UI.Timelines;

namespace Zoorce.Web.UI.Tests;

/// <summary>Exercises validation edges retained behind the public form and timeline APIs.</summary>
public sealed class FormAndTimelineEdgeTests
{
    [Fact]
    public void File_descriptors_and_chips_normalize_every_optional_value_and_reject_invalid_input()
    {
        var file = new ZoorceFileUploadItem(" file ", " release.png ", " /assets/release.png ", " IMAGE/PNG ", 42).Normalize();
        Assert.Equal("file", file.Id);
        Assert.Equal("release.png", file.Name);
        Assert.Equal("/assets/release.png", file.Url);
        Assert.Equal("image/png", file.ContentType);

        Assert.Throws<ArgumentException>(() => new ZoorceFileUploadItem(" ", "file").Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFileUploadItem("file", " ").Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFileUploadItem("file", "file", SizeBytes: -1).Normalize());

        var chip = new ZoorceChipOptions { Text = " Build ", Icon = " ✓ ", Tone = " SUCCESS ", AriaLabel = " Build status " }.Normalize();
        Assert.Equal("Build", chip.Text);
        Assert.Equal("✓", chip.Icon);
        Assert.Equal("success", chip.Tone);
        Assert.Equal("Build status", chip.AriaLabel);
        Assert.Throws<ArgumentException>(() => new ZoorceChipOptions { Text = " " }.Normalize());
    }

    [Fact]
    public void Internal_field_render_model_validates_all_guarded_values_and_normalizes_otp()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", Kind = (ZoorceFormControlKind)999 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", State = (ZoorceFormControlState)999 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", MaximumLength = 0 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", MaximumFiles = 0 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", MaximumTotalFileSizeBytes = 0 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", FileUploadBehavior = (ZoorceFileUploadBehavior)999 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", OtpCharacterSet = (ZoorceOtpCharacterSet)999 }.Normalize());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZoorceFormControlOptions { Id = "field", OtpTextTransform = (ZoorceOtpTextTransform)999 }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "field", Choices = [null!] }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "field", ExistingFiles = [null!] }.Normalize());
        Assert.Throws<ArgumentException>(() => new ZoorceFormControlOptions { Id = "field", MaximumFiles = 1, ExistingFiles = [new("one", "one"), new("two", "two")] }.Normalize());

        var otp = new ZoorceFormControlOptions
        {
            Id = " code ",
            Kind = ZoorceFormControlKind.Otp,
            CodeLength = 4,
            Value = " a1-b2c3 ",
            OtpCharacterSet = ZoorceOtpCharacterSet.Alphanumeric,
            OtpTextTransform = ZoorceOtpTextTransform.Lowercase,
            Metadata = new Dictionary<string, string?> { [" keep "] = " value ", [" "] = "ignored" }
        }.Normalize();

        Assert.Equal("a1b2", otp.Value);
        Assert.Equal(4, otp.GetOtpLength());
        Assert.Equal(["a1b2"], otp.GetSelectedValues());
        Assert.Equal("value", otp.Metadata["keep"]);
    }

    [Fact]
    public void Timeline_template_context_exposes_present_and_missing_metadata()
    {
        var timeline = new ZoorceTimelineOptions
        {
            Id = "timeline",
            Items = [new ZoorceTimelineItem("one", DateTimeOffset.UtcNow, "One", Metadata: new Dictionary<string, string> { ["owner"] = "Ada" })]
        }.Normalize();
        var context = new ZoorceTimelineItemTemplateContext(timeline, timeline.Items.Single(), 0, true, "Today", null, ZoorceTimelineItemClickBehavior.Select);

        Assert.Equal("Ada", context.Metadata("owner"));
        Assert.Null(context.Metadata("missing"));
        Assert.Equal(ZoorceTimelineItemClickBehavior.Select, context.ClickBehavior);
    }
}
