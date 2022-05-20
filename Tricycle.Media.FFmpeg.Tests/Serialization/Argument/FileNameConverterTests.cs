using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument;

[TestClass]
public class FileNameConverterTests
{
    FileNameConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new FileNameConverter();
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void ConvertThrowsExceptionWhenValueIsNotAString()
    {
        _converter.Convert("-i", 0);
    }

    [TestMethod]
    public void ConvertEscapesFilePath()
    {
        string argName = "-i";
        string fileName = "Users/fred/Movies/movie.mkv";
        Assert.AreEqual($"{argName} \"{fileName}\"", _converter.Convert(argName, fileName));
    }
}
