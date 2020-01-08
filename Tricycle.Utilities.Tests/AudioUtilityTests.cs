using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Utilities.Tests
{
    [TestClass]
    public class AudioUtilityTests
    {
        [TestMethod]
        public void TestGetChannelCount()
        {
            Assert.AreEqual(1, AudioUtility.GetChannelCount(AudioMixdown.Mono));
            Assert.AreEqual(2, AudioUtility.GetChannelCount(AudioMixdown.Stereo));
            Assert.AreEqual(6, AudioUtility.GetChannelCount(AudioMixdown.Surround5dot1));
        }
    }
}
