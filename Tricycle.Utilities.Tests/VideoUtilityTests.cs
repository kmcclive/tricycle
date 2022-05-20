using Tricycle.Models;

namespace Tricycle.Utilities.Tests;

[TestClass]
public class VideoUtilityTests
{
    [TestMethod]
    public void TestSupportsHdr()
    {
        Assert.IsTrue(VideoUtility.SupportsHdr(VideoFormat.Hevc));
        Assert.IsFalse(VideoUtility.SupportsHdr(VideoFormat.Avc));
    }

    [TestMethod]
    public void TestGetAspectRatio()
    {
        Assert.AreEqual(2.4, VideoUtility.GetAspectRatio(new Dimensions(1920, 800)));
        Assert.AreEqual(1.778, VideoUtility.GetAspectRatio(new Dimensions(3840, 2160)), 0.001);
        Assert.AreEqual(1.333, VideoUtility.GetAspectRatio(new Dimensions(640, 480)), 0.001);
    }

    [TestMethod]
    public void TestGetSampleAspectRatio()
    {
        Assert.AreEqual(0.889,
                        VideoUtility.GetSampleAspectRatio(new Dimensions(640, 480), new Dimensions(720, 480)),
                        0.001);
        Assert.AreEqual(1.185,
                        VideoUtility.GetSampleAspectRatio(new Dimensions(853, 480), new Dimensions(720, 480)),
                        0.001);
        Assert.AreEqual(1,
                        VideoUtility.GetSampleAspectRatio(new Dimensions(1920, 1080), new Dimensions(1920, 1080)));
    }

    [TestMethod]
    public void TestGetWidth()
    {
        Assert.AreEqual(1920, VideoUtility.GetWidth(800, 2.4));
        Assert.AreEqual(3840, VideoUtility.GetWidth(2160, 16 / 9d));
        Assert.AreEqual(640, VideoUtility.GetWidth(480, 4 / 3d));
    }

    [TestMethod]
    public void TestGetHeight()
    {
        Assert.AreEqual(800, VideoUtility.GetHeight(1920, 2.4));
        Assert.AreEqual(2160, VideoUtility.GetHeight(3840, 16 / 9d));
        Assert.AreEqual(480, VideoUtility.GetHeight(640, 4 / 3d));
    }
}
