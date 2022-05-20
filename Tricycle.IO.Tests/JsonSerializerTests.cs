using System;
using System.Runtime.Serialization;

namespace Tricycle.IO.Tests;

public class JsonSerializerTests
{
    #region Nested Types

    class MockObject
    {
        public int Value { get; set; }
    }

    #endregion

    #region Fields

    JsonSerializer _serializer;
    MockObject _value = new MockObject()
    {
        Value = 2
    };
    string _json =
        "{" + Environment.NewLine +
        "  \"value\": 2" + Environment.NewLine +
        "}";

    #endregion

    #region Test Setup

    [TestInitialize]
    public void Setup()
    {
        _serializer = new JsonSerializer();
    }

    #endregion

    #region Test Methods

    [TestMethod]
    public void SerializeReturnsJsonString()
    {
        var actual = _serializer.Serialize(_value);

        Assert.AreEqual(_json, actual);
    }

    [TestMethod]
    [ExpectedException(typeof(SerializationException))]
    public void DeserializeThrowsSerializationException()
    {
        _serializer.Deserialize<MockObject>("{");
    }

    [TestMethod]
    public void DeserializeReturnsObject()
    {
        var actual = _serializer.Deserialize<MockObject>(_json);

        Assert.AreEqual(_value, actual);
    }

    #endregion
}
