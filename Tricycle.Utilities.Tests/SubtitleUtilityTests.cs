using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Models;

namespace Tricycle.Utilities.Tests
{
    [TestClass]
    public class SubtitleUtilityTests
    {
        [TestMethod]
        public void TestIsSupportedByContainer()
        {
            foreach (SubtitleFormat subtitleFormat in Enum.GetValues(typeof(SubtitleFormat)))
            {
                var isSupported = SubtitleUtility.IsSupportedByContainer(ContainerFormat.Mkv, subtitleFormat);

                Assert.AreEqual(subtitleFormat != SubtitleFormat.TimedText, isSupported);

                isSupported = SubtitleUtility.IsSupportedByContainer(ContainerFormat.Mp4, subtitleFormat);

                Assert.AreEqual(subtitleFormat == SubtitleFormat.TimedText, isSupported);
            }
        }
    }
}
