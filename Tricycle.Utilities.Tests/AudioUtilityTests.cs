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

        [TestMethod]
        public void TestGetAudioFormat()
        {
            var stream = new AudioStreamInfo();

            Assert.IsNull(AudioUtility.GetAudioFormat(stream));

            stream.FormatName = "DTS";

            Assert.IsNull(AudioUtility.GetAudioFormat(stream));

            stream.FormatName = "ac-3";

            Assert.AreEqual(AudioFormat.Ac3, AudioUtility.GetAudioFormat(stream));

            stream.FormatName = "aac";

            Assert.AreEqual(AudioFormat.Aac, AudioUtility.GetAudioFormat(stream));

            stream.ProfileName = "he";

            Assert.AreEqual(AudioFormat.HeAac, AudioUtility.GetAudioFormat(stream));
        }
    }
}
