using System.Text;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Previewers.Text;

namespace UniversalQuickPreview.Previewers.Tests.Text;

public sealed class TextPreviewProviderTests
{
    [Theory]
    [InlineData("sample.txt")]
    [InlineData("sample.cs")]
    [InlineData("sample.JSON")]
    [InlineData("sample.md")]
    public void CanPreview_RecognizesSupportedExtensions(string fileName)
    {
        Assert.True(new TextPreviewProvider().CanPreview(fileName));
    }

    [Fact]
    public async Task LoadAsync_DetectsUtf16Bom()
    {
        var path = GetTemporaryPath(".txt");
        try
        {
            await File.WriteAllTextAsync(path, "繁體中文 preview", Encoding.Unicode);

            var result = await new TextPreviewProvider().LoadAsync(path, CancellationToken.None);

            Assert.Equal(PreviewContentKind.Text, result.Kind);
            Assert.Equal("繁體中文 preview", result.Text);
            Assert.False(result.IsTruncated);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_TruncatesLargeTextSafely()
    {
        var path = GetTemporaryPath(".log");
        try
        {
            await File.WriteAllTextAsync(path, new string('x', 300_000));

            var result = await new TextPreviewProvider().LoadAsync(path, CancellationToken.None);

            Assert.True(result.IsTruncated);
            Assert.Equal(262_144, result.Text!.Length);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_HonorsCancellation()
    {
        var path = GetTemporaryPath(".txt");
        try
        {
            await File.WriteAllTextAsync(path, "preview");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => new TextPreviewProvider().LoadAsync(path, cancellation.Token));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string GetTemporaryPath(string extension)
    {
        return Path.Combine(Path.GetTempPath(), $"UniversalQuickPreview-{Guid.NewGuid():N}{extension}");
    }
}
