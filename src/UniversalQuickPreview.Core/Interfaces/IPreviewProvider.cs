using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IPreviewProvider
{
    string Name { get; }

    int Priority { get; }

    IReadOnlyCollection<string> SupportedExtensions { get; }

    bool CanPreview(string filePath);

    Task<PreviewContent> LoadAsync(string filePath, CancellationToken cancellationToken);
}
