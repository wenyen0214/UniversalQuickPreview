using System.Diagnostics;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Services;

public sealed class PreviewProviderRegistry(IEnumerable<IPreviewProvider> providers) : IPreviewProviderRegistry
{
    public IReadOnlyList<IPreviewProvider> Providers { get; } = providers
        .OrderByDescending(provider => provider.Priority)
        .ToArray();

    public IPreviewProvider Resolve(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        return Providers.FirstOrDefault(provider => provider.CanPreview(filePath))
            ?? throw new NotSupportedException($"No preview provider accepted '{Path.GetExtension(filePath)}'.");
    }

    public async Task<PreviewLoadResult> LoadAsync(string filePath, CancellationToken cancellationToken)
    {
        var provider = Resolve(filePath);
        var stopwatch = Stopwatch.StartNew();
        var content = await provider.LoadAsync(filePath, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();
        return new PreviewLoadResult(provider.Name, content, stopwatch.Elapsed);
    }
}
