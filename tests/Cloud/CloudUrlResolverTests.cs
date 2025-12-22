using SlideGenerator.Framework.Cloud;

namespace SlideGenerator.Framework.Tests.Cloud;

[TestClass]
public class CloudUrlResolverTests
{
    [TestMethod]
    [DataRow("https://drive.google.com/file/d/1abc123/view", true)]
    [DataRow("https://1drv.ms/i/s!abc", true)]
    [DataRow("https://photos.app.goo.gl/abc", true)]
    [DataRow("https://example.com/image.png", false)]
    public void IsCloudUrlSupported_ReturnsCorrectResult(string url, bool expected)
    {
        // Act
        var result = CloudUrlResolver.IsCloudUrlSupported(new Uri(url));

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public async Task ResolveLinkAsync_GoogleDriveFile_ReturnsDirectLink()
    {
        // Arrange
        var url = "https://drive.google.com/file/d/1abc123/view";

        // Act
        var result = await CloudUrlResolver.ResolveLinkAsync(url);

        // Assert
        Assert.AreEqual("https://drive.google.com/uc?export=download&id=1abc123", result.ToString());
    }

    [TestMethod]
    public void ResolveLinkAsync_OneDrive_ReturnsDirectLink()
    {
        // Arrange
        var url = "https://1drv.ms/i/s!abc";
        // Base64 of "https://1drv.ms/i/s!abc" is aHR0cHM6Ly8xZHJ2Lm1zL2kicyFhYmM

        // Act
        var result = CloudUrlResolver.ResolveLinkAsync(url).GetAwaiter().GetResult();

        // Assert
        StringAssert.StartsWith(result.ToString(), "https://api.onedrive.com/v1.0/shares/u!");
    }
}