using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IPreviewProviderRegistry
{
    IReadOnlyList<IPreviewProvider> Providers { get; }

    IPreviewProvider Resolve(string filePath);

    Task<PreviewLoadResult> LoadAsync(string filePath, CancellationToken cancellationToken);
}
