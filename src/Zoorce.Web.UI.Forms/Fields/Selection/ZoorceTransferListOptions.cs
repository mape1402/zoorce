namespace Zoorce.Web.UI.Forms;

/// <summary>Configures a two-list transfer selector.</summary>
public sealed record ZoorceTransferListOptions : ZoorceChoiceFieldOptions
{
    /// <inheritdoc />
    private protected override ZoorceFormControlOptions CreateRenderOptions()
        => CreateChoiceRenderOptions() with { Kind = ZoorceFormControlKind.TransferList };
}
