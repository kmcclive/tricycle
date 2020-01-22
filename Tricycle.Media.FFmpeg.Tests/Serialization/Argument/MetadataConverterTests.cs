using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class MetadataConverterTests
    {
        MetadataConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new MetadataConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotDictionary()
        {
            _converter.Convert("-metadata", 0);
        }

        [TestMethod]
        public void ConvertIncludesKeyAndValue()
        {
            var metadata = new Dictionary<string, string>()
            {
                { "title", "Stereo" }
            };

            Assert.AreEqual("-metadata title=\"Stereo\"", _converter.Convert("-metadata", metadata));
        }

        [TestMethod]
        public void ConvertEscapesValue()
        {
            var metadata = new Dictionary<string, string>()
            {
                { "title", "Charlotte's Web" }
            };
            var expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                           ? "-metadata title=\"Charlotte\\\\\\\'s Web\""
                           : "-metadata title=\"Charlotte\\\\\\\\\\\'s Web\"";

            Assert.AreEqual(expected, _converter.Convert("-metadata", metadata));
        }

        [TestMethod]
        public void ConvertIncludesArgumentNameForEachValue()
        {
            var metadata = new Dictionary<string, string>()
            {
                { "title", "Stereo" },
                { "language", "eng" }
            };

            Assert.AreEqual("-metadata title=\"Stereo\" -metadata language=\"eng\"",
                            _converter.Convert("-metadata", metadata));
        }
    }
}
