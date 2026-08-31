using System.Text;
using System.IO;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Previewers.Text;

public sealed class TextPreviewProvider : IPreviewProvider
{
    internal const int MaximumCharacters = 262_144;

    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".ini", ".cfg", ".conf",
        ".cs", ".cpp", ".c", ".h", ".hpp", ".py", ".js", ".ts",
        ".html", ".htm", ".css", ".xml", ".yaml", ".yml", ".json",
        ".md", ".markdown", ".csv", ".tsv", ".sql", ".ps1", ".sh"
    };

    public string Name => "Text";

    public int Priority => 100;

    public IReadOnlyCollection<string> SupportedExtensions => Extensions;

    public bool CanPreview(string filePath)
    {
        return Extensions.Contains(Path.GetExtension(filePath));
    }

    public async Task<PreviewContent> LoadAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 16_384,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 16_384,
            leaveOpen: false);

        var buffer = new char[MaximumCharacters + 1];
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        var isTruncated = totalRead > MaximumCharacters;
        var text = new string(buffer, 0, Math.Min(totalRead, MaximumCharacters));
        return new PreviewContent(PreviewContentKind.Text, Text: text, IsTruncated: isTruncated);
    }
}
