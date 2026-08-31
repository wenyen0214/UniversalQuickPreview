using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Core.Services;

namespace UniversalQuickPreview.Core.Tests.Services;

public sealed class PreviewProviderRegistryTests
{
    [Fact]
    public void Resolve_UsesHighestPriorityMatchingProvider()
    {
        var lower = new StubProvider("Lower", 10, true);
        var higher = new StubProvider("Higher", 20, true);
        var registry = new PreviewProviderRegistry([lower, higher]);

        Assert.Same(higher, registry.Resolve("sample.txt"));
    }

    [Fact]
    public async Task LoadAsync_ReturnsProviderNameAndContent()
    {
        var provider = new StubProvider("Text", 10, true);
        var registry = new PreviewProviderRegistry([provider]);

        var result = await registry.LoadAsync("sample.txt", CancellationToken.None);

        Assert.Equal("Text", result.ProviderName);
        Assert.Equal("preview", result.Content.Text);
        Assert.True(result.LoadDuration >= TimeSpan.Zero);
    }

    private sealed class StubProvider(string name, int priority, bool canPreview) : IPreviewProvider
    {
        public string Name { get; } = name;

        public int Priority { get; } = priority;

        public IReadOnlyCollection<string> SupportedExtensions => [".txt"];

        public bool CanPreview(string filePath) => canPreview;

        public Task<PreviewContent> LoadAsync(string filePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(new PreviewContent(PreviewContentKind.Text, Text: "preview"));
        }
    }
}
