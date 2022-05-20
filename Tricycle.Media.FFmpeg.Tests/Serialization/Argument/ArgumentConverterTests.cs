using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument;

[TestClass]
public class ArgumentConverterTests
{
    ArgumentConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new ArgumentConverter();
    }

    [TestMethod]
    public void ConvertIncludesArgName()
    {
        string argName = "-preset";

        Assert.AreEqual(argName, _converter.Convert(argName, null));
    }

    [TestMethod]
    public void ConvertIncludesValue()
    {
        string value = "medium";

        Assert.AreEqual(value, _converter.Convert(null, value));
    }

    [TestMethod]
    public void ConvertIncludesArgNameAndValue()
    {
        var converter = new ArgumentConverter();

        Assert.AreEqual("-preset medium", converter.Convert("-preset", "medium"));
    }
}
