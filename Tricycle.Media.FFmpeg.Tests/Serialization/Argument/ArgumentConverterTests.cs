using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class ArgumentConverterTests
    {
        [TestMethod]
        public void TestConvert()
        {
            var converter = new ArgumentConverter();

            Assert.AreEqual("-preset medium", converter.Convert("-preset", "medium"));
        }
    }
}
