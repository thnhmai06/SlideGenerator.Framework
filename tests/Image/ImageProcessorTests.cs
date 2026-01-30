using System.Drawing;
using SlideGenerator.Framework.Image;

namespace SlideGenerator.Framework.Tests.Image;

[TestClass]
public class ImageUtilitiesTests
{
    [TestMethod]
    public void GetMaxAspectSize_TargetWider_ReturnsCorrectSize()
    {
        // Arrange
        var original = new Size(1000, 1000);
        var target = new Size(1600, 900); // 16:9

        // Act
        var result = ManipulatingService.GetMaxAspectSize(original, target);

        // Assert
        Assert.AreEqual(1000, result.Width);
        Assert.AreEqual(562, result.Height); // 1000 * 9 / 16 = 562.5 -> ToEven = 562
    }

    [TestMethod]
    public void GetMaxAspectSize_TargetTaller_ReturnsCorrectSize()
    {
        // Arrange
        var original = new Size(1000, 1000);
        var target = new Size(900, 1600); // 9:16

        // Act
        var result = ManipulatingService.GetMaxAspectSize(original, target);

        // Assert
        Assert.AreEqual(562, result.Width); // 1000 * 9 / 16 = 562.5 -> ToEven = 562
        Assert.AreEqual(1000, result.Height);
    }
}