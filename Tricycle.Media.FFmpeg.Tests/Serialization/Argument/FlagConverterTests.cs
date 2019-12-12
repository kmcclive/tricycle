using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class FlagConverterTests
    {
        FlagConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new FlagConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotBool()
        {
            _converter.Convert("-y", 0);
        }

        [TestMethod]
        public void ConvertIncludesFlagWhenTrue()
        {
            string flag = "-y";

            Assert.AreEqual(flag, _converter.Convert(flag, true));
        }

        [TestMethod]
        public void ConvertDoesNotIncludeFlagWhenFalse()
        {
            Assert.AreEqual(string.Empty, _converter.Convert("-y", false));
        }
    }
}
