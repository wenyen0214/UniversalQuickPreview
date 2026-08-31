using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using System.IO;

namespace UniversalQuickPreview.Previewers.Fallback;

public sealed class FallbackPreviewProvider : IPreviewProvider
{
    public string Name => "Fallback";

    public int Priority => int.MinValue;

    public IReadOnlyCollection<string> SupportedExtensions => [];

    public bool CanPreview(string filePath)
    {
        return true;
    }

    public Task<PreviewContent> LoadAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var extension = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();
        var detail = string.IsNullOrEmpty(extension) ? "FILE" : extension;
        return Task.FromResult(new PreviewContent(PreviewContentKind.Unsupported, Detail: detail));
    }
}
