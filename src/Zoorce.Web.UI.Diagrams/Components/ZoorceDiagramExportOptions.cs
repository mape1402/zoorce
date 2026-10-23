namespace Zoorce.Web.UI.Diagrams;

/// <summary>Configures the portable files an individual diagram can create.</summary>
public sealed record ZoorceDiagramExportOptions
{
    private static readonly ZoorceDiagramExportFormat[] DefaultFormats =
    [
        ZoorceDiagramExportFormat.Json,
        ZoorceDiagramExportFormat.Svg,
        ZoorceDiagramExportFormat.Jpeg,
        ZoorceDiagramExportFormat.Html,
        ZoorceDiagramExportFormat.Pdf
    ];

    /// <summary>Gets whether this diagram exposes the export experience.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Gets the formats exposed in the export modal.</summary>
    public IReadOnlyList<ZoorceDiagramExportFormat>? AllowedFormats { get; init; } = DefaultFormats;

    /// <summary>Gets the initially selected export format.</summary>
    public ZoorceDiagramExportFormat DefaultFormat { get; init; } = ZoorceDiagramExportFormat.Svg;

    /// <summary>Gets the suggested download file name without an extension.</summary>
    public string? FileName { get; init; }

    /// <summary>Gets the document-unit padding around exported content.</summary>
    public int DefaultPadding { get; init; } = 48;

    /// <summary>Gets the default JPEG pixel-density multiplier.</summary>
    public int DefaultJpegScale { get; init; } = 2;

    /// <summary>Gets the pixel-density choices exposed for JPEG files.</summary>
    public IReadOnlyList<int>? JpegScales { get; init; } = [1, 2, 3];

    /// <summary>Gets the initial export background strategy.</summary>
    public ZoorceDiagramExportBackground DefaultBackground { get; init; } = ZoorceDiagramExportBackground.Transparent;

    /// <summary>Gets the fallback CSS color used when a solid export background is requested.</summary>
    public string? SolidBackground { get; init; }

    /// <summary>Gets the browser or server strategy used for PDF output.</summary>
    public ZoorceDiagramPdfMode PdfMode { get; init; } = ZoorceDiagramPdfMode.BrowserPrint;

    /// <summary>Gets an optional host endpoint that returns a downloadable PDF.</summary>
    public string? PdfExportUrl { get; init; }

    /// <summary>Produces a safe, complete configuration for the browser component.</summary>
    public ZoorceDiagramExportOptions Normalize(string title)
    {
        var formats = (AllowedFormats ?? DefaultFormats)
            .Where(Enum.IsDefined)
            .Distinct()
            .ToArray();
        if (formats.Length == 0)
        {
            formats = DefaultFormats;
        }

        var scales = (JpegScales ?? [1, 2, 3])
            .Where(scale => scale is >= 1 and <= 4)
            .Distinct()
            .Order()
            .ToArray();
        if (scales.Length == 0)
        {
            scales = [1, 2, 3];
        }

        var fileName = TrimOrNull(FileName) ?? TrimOrNull(title) ?? "diagram";
        var pdfUrl = TrimOrNull(PdfExportUrl);
        var pdfMode = Enum.IsDefined(PdfMode) && PdfMode == ZoorceDiagramPdfMode.Server && pdfUrl is not null
            ? ZoorceDiagramPdfMode.Server
            : ZoorceDiagramPdfMode.BrowserPrint;
        var background = Enum.IsDefined(DefaultBackground) ? DefaultBackground : ZoorceDiagramExportBackground.Transparent;
        var requestedFormat = Enum.IsDefined(DefaultFormat) ? DefaultFormat : ZoorceDiagramExportFormat.Svg;

        return this with
        {
            AllowedFormats = formats,
            DefaultFormat = formats.Contains(requestedFormat) ? requestedFormat : formats[0],
            FileName = fileName,
            DefaultPadding = Math.Clamp(DefaultPadding, 0, 240),
            JpegScales = scales,
            DefaultJpegScale = scales.Contains(DefaultJpegScale) ? DefaultJpegScale : scales.OrderBy(scale => Math.Abs(scale - DefaultJpegScale)).First(),
            DefaultBackground = background,
            SolidBackground = TrimOrNull(SolidBackground),
            PdfMode = pdfMode,
            PdfExportUrl = pdfMode == ZoorceDiagramPdfMode.Server ? pdfUrl : null
        };
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
