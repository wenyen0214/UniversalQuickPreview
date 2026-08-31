using System.Globalization;
using UniversalQuickPreview.Core.Formatting;

namespace UniversalQuickPreview.Core.Tests.Formatting;

public sealed class FileSizeFormatterTests
{
    private static readonly CultureInfo TestCulture = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(5_242_880, "5 MB")]
    [InlineData(1_073_741_824, "1 GB")]
    public void Format_ReturnsReadableBinarySize(long bytes, string expected)
    {
        Assert.Equal(expected, FileSizeFormatter.Format(bytes, TestCulture));
    }

    [Fact]
    public void Format_RejectsNegativeSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileSizeFormatter.Format(-1, TestCulture));
    }
}
