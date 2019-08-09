using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Models;

namespace Tricycle.Utilities.Tests
{
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
    }
}
