using UniversalQuickPreview.Core.Services;

namespace UniversalQuickPreview.Core.Tests.Services;

public sealed class FileMetadataServiceTests
{
    [Fact]
    public void GetMetadata_ReadsFileWithoutChangingIt()
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), $"UniversalQuickPreview-{Guid.NewGuid():N}");
        var testFile = Path.Combine(testDirectory, "sample.txt");
        Directory.CreateDirectory(testDirectory);

        try
        {
            File.WriteAllText(testFile, "preview");
            var originalBytes = File.ReadAllBytes(testFile);
            var service = new FileMetadataService();

            var metadata = service.GetMetadata(testFile);

            Assert.Equal("sample.txt", metadata.Name);
            Assert.Equal(".txt", metadata.Extension);
            Assert.Equal(originalBytes.Length, metadata.SizeBytes);
            Assert.Equal(Path.GetFullPath(testFile), metadata.FullPath);
            Assert.Equal(originalBytes, File.ReadAllBytes(testFile));
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public void GetMetadata_RejectsMissingFile()
    {
        var service = new FileMetadataService();
        var missingFile = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.txt");

        Assert.Throws<FileNotFoundException>(() => service.GetMetadata(missingFile));
    }
}
