using System.Windows.Media.Imaging;
using System.IO;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Previewers.Image;

public sealed class ImagePreviewProvider : IPreviewProvider
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif"
    };

    public string Name => "Image";

    public int Priority => 200;

    public IReadOnlyCollection<string> SupportedExtensions => Extensions;

    public bool CanPreview(string filePath)
    {
        return Extensions.Contains(Path.GetExtension(filePath));
    }

    public Task<PreviewContent> LoadAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 4_096,
            FileOptions.SequentialScan);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
        var frame = decoder.Frames.FirstOrDefault() ?? throw new InvalidDataException("The image contains no frames.");
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new PreviewContent(
            PreviewContentKind.Image,
            SourcePath: filePath,
            PixelWidth: frame.PixelWidth,
            PixelHeight: frame.PixelHeight));
    }
}
