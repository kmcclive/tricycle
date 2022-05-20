using System;
using System.Linq;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument;

[TestClass]
public class ArgumentPropertyReflectorTests
{
    class MockConverter : IArgumentConverter
    {
        public IArgumentPropertyReflector Reflector { get; set; }

        public string Convert(string argName, object value)
        {
            throw new NotImplementedException();
        }
    }

    class MockObject
    {
        string A { get; set; } = "a";

        public string B { get; set; } = "b";

        [Argument("-z")]
        [ArgumentOrder(1)]
        public string C { get; set; } = "c";

        [ArgumentOrder(0)]
        [ArgumentConverter(typeof(MockConverter))]
        public string D { get; set; } = "d";

        [ArgumentIgnore]
        public string E { get; set; } = "e";

        public string F { get; set; }
    }

    ArgumentPropertyReflector _reflector;
    MockObject _obj;

    [TestInitialize]
    public void Setup()
    {
        _reflector = new ArgumentPropertyReflector();
        _obj = new MockObject();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ReflectThrowsExceptionWhenObjIsNull()
    {
        _reflector.Reflect(null);
    }

    [TestMethod]
    public void ReflectIgnoresPrivateProperties()
    {
        var properties = _reflector.Reflect(_obj);

        Assert.IsFalse(properties.Any(p => p?.PropertyName == "A"), "The private property was returned.");
    }

    [TestMethod]
    public void ReflectIgnoresNullValues()
    {
        var properties = _reflector.Reflect(_obj);

        Assert.IsFalse(properties.Any(p => p?.PropertyName == "F"), "The property with null value was returned.");
    }

    [TestMethod]
    public void ReflectUsesArgumentIgnore()
    {
        var properties = _reflector.Reflect(_obj);

        Assert.IsFalse(properties.Any(p => p?.PropertyName == "E"),
                       "The property with ArgumentIgnore attribute was returned.");
    }

    [TestMethod]
    public void ReflectUsesArgumentOrder()
    {
        var properties = _reflector.Reflect(_obj);
        int? cIndex = null;
        int? dIndex = null;

        for (int i = 0; i < (properties?.Count ?? 0); i++)
        {
            var property = properties[i];

            switch (property.PropertyName)
            {
                case "C":
                    cIndex = i;
                    break;
                case "D":
                    dIndex = i;
                    break;
            }
        }

        Assert.IsTrue(dIndex < cIndex, "The properties with ArgumentOrder attributes weren't ordered correctly.");
    }

    [TestMethod]
    public void ReflectUsesArgumentAttribute()
    {
        var properties = _reflector.Reflect(_obj);
        var property = properties.FirstOrDefault(p => p?.PropertyName == "C");

        Assert.AreEqual("-z", property?.ArgumentName);
    }

    [TestMethod]
    public void ReflectUsesArgumentConverter()
    {
        var properties = _reflector.Reflect(_obj);
        var property = properties.FirstOrDefault(p => p?.PropertyName == "D");

        Assert.IsInstanceOfType(property?.Converter, typeof(MockConverter));
    }

    [TestMethod]
    public void ReflectAssignsDefaultConverter()
    {
        var properties = _reflector.Reflect(_obj);
        var property = properties.FirstOrDefault(p => p?.PropertyName == "B");

        Assert.IsInstanceOfType(property?.Converter, typeof(ArgumentConverter));
    }

    [TestMethod]
    public void ReflectAssignsReflectorToConverters()
    {
        var properties = _reflector.Reflect(_obj);
        var property = properties.FirstOrDefault(p => p?.PropertyName == "B");

        Assert.AreEqual(_reflector, property?.Converter?.Reflector);
    }

    [TestMethod]
    public void ReflectAssignsPropertyValues()
    {
        var properties = _reflector.Reflect(_obj);

        foreach (var property in properties)
        {
            Assert.AreEqual(property.PropertyName.ToLower(), property.Value);
        }
    }
}
