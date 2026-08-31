namespace UniversalQuickPreview.Core.Models;

public enum PreviewContentKind
{
    Text,
    Image,
    Unsupported
}

public sealed record PreviewContent(
    PreviewContentKind Kind,
    string? Text = null,
    string? SourcePath = null,
    int? PixelWidth = null,
    int? PixelHeight = null,
    bool IsTruncated = false,
    string? Detail = null);

public sealed record PreviewLoadResult(
    string ProviderName,
    PreviewContent Content,
    TimeSpan LoadDuration);
