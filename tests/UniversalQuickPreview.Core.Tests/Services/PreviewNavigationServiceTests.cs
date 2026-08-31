using UniversalQuickPreview.Core.Services;

namespace UniversalQuickPreview.Core.Tests.Services;

public sealed class PreviewNavigationServiceTests
{
    [Fact]
    public void Move_NavigatesWithinFolderWithoutWrapping()
    {
        var navigation = new PreviewNavigationService();
        navigation.Start(["a.txt", "b.txt", "c.txt"], 1);

        Assert.Equal("b.txt", navigation.Current);
        Assert.Equal("c.txt", navigation.Move(1));
        Assert.Equal("c.txt", navigation.Move(1));
        Assert.Equal("b.txt", navigation.Move(-1));
        Assert.Equal("a.txt", navigation.Move(-1));
        Assert.Equal("a.txt", navigation.Move(-1));
    }

    [Fact]
    public void Start_ClampsInvalidSelectedIndex()
    {
        var navigation = new PreviewNavigationService();

        navigation.Start(["a.txt", "b.txt"], 99);

        Assert.Equal("b.txt", navigation.Current);
    }
}
