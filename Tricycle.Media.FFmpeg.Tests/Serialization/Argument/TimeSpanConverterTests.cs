using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument;

[TestClass]
public class TimeSpanConverterTests
{
    TimeSpanConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new TimeSpanConverter();
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void ConvertThrowsExceptionWhenValueIsNotTimeSpan()
    {
        _converter.Convert("-ss", 0);
    }

    [TestMethod]
    public void ConvertAlwaysIncludesMinutesAndSeconds()
    {
        Assert.AreEqual("-ss 00:00", _converter.Convert("-ss", new TimeSpan(0)));
    }

    [TestMethod]
    public void ConvertIncludesHours()
    {
        Assert.AreEqual("-ss 01:27:00", _converter.Convert("-ss", new TimeSpan(1, 27, 0)));
    }

    [TestMethod]
    public void ConvertIncludesMilliseconds()
    {
        Assert.AreEqual("-ss 27:00.009", _converter.Convert("-ss", new TimeSpan(0, 0, 27, 0, 9)));
    }

    [TestMethod]
    public void ConvertIncludesAllComponents()
    {
        Assert.AreEqual("-ss 01:27:13.485", _converter.Convert("-ss", new TimeSpan(0, 1, 27, 13, 485)));
    }
}
