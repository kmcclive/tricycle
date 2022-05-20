using System;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Utilities.Tests;

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

    [TestMethod]
    public void TestGetPreferredFormat()
    {
        Assert.AreEqual(SubtitleFormat.Ssa,
            SubtitleUtility.GetPreferredFormat(ContainerFormat.Mkv, SubtitleType.Text));
        Assert.IsNull(SubtitleUtility.GetPreferredFormat(ContainerFormat.Mkv, SubtitleType.Graphic));

        Assert.AreEqual(SubtitleFormat.TimedText,
            SubtitleUtility.GetPreferredFormat(ContainerFormat.Mp4, SubtitleType.Text));
        Assert.IsNull(SubtitleUtility.GetPreferredFormat(ContainerFormat.Mp4, SubtitleType.Graphic));
    }
}
