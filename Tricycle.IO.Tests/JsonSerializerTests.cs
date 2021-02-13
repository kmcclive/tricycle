using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tricycle.IO.Tests
{
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
        public void TestSerialize()
        {
            var actual = _serializer.Seriialize(_value);

            Assert.AreEqual(_json, actual);
        }

        [TestMethod]
        public void TestDeserialize()
        {
            var actual = _serializer.Deserialize<MockObject>(_json);

            Assert.AreEqual(_value, actual);
        }

        #endregion
    }
}
