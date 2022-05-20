using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument;

[TestClass]
public class BinaryConverterTests
{
    BinaryConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new BinaryConverter();
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void ConvertThrowsExceptionWhenValueIsNotABool()
    {
        _converter.Convert("-forced_subs_only", 0);
    }

    [TestMethod]
    public void ConvertUsesZeroForFalse()
    {
        Assert.AreEqual("-forced_subs_only 0", _converter.Convert("-forced_subs_only", false));
    }

    [TestMethod]
    public void ConvertUsesOneForTrue()
    {
        Assert.AreEqual("-forced_subs_only 1", _converter.Convert("-forced_subs_only", true));
    }
}
