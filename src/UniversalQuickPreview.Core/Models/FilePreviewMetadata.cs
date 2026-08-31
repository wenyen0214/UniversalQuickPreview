namespace UniversalQuickPreview.Core.Models;

public sealed record FilePreviewMetadata(
    string Name,
    string FullPath,
    string Extension,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime ModifiedAt);
