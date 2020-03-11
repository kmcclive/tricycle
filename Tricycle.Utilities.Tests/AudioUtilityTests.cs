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
            Assert.AreEqual(8, AudioUtility.GetChannelCount(AudioMixdown.Surround7dot1));
        }

        [TestMethod]
        public void TestGetMixdown()
        {
            Assert.AreEqual(AudioMixdown.Mono, AudioUtility.GetMixdown(1));
            Assert.AreEqual(AudioMixdown.Stereo, AudioUtility.GetMixdown(2));
            Assert.AreEqual(AudioMixdown.Surround5dot1, AudioUtility.GetMixdown(6));
            Assert.AreEqual(AudioMixdown.Surround7dot1, AudioUtility.GetMixdown(8));
            Assert.AreEqual(null, AudioUtility.GetMixdown(4));
        }

        [TestMethod]
        public void TestIsSupportedByContainer()
        {
            foreach (AudioFormat audioFormat in Enum.GetValues(typeof(AudioFormat)))
            {
                Assert.IsTrue(AudioUtility.IsSupportedByContainer(ContainerFormat.Mkv, audioFormat));
            }

            Assert.IsTrue(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.Aac));
            Assert.IsTrue(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.Ac3));
            Assert.IsTrue(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.Dts));
            Assert.IsTrue(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.HeAac));
            Assert.IsFalse(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.DolbyTrueHd));
            Assert.IsFalse(AudioUtility.IsSupportedByContainer(ContainerFormat.Mp4, AudioFormat.DtsMasterAudio));
        }
    }
}
