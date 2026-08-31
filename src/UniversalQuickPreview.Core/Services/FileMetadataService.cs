using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Services;

public sealed class FileMetadataService : IFileMetadataService
{
    public FilePreviewMetadata GetMetadata(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var file = new FileInfo(filePath);
        if (!file.Exists)
        {
            throw new FileNotFoundException("The selected file no longer exists.", filePath);
        }

        return new FilePreviewMetadata(
            file.Name,
            file.FullName,
            file.Extension,
            file.Length,
            file.CreationTime,
            file.LastWriteTime);
    }
}
