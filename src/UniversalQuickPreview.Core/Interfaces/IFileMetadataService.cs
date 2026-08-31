using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IFileMetadataService
{
    FilePreviewMetadata GetMetadata(string filePath);
}
