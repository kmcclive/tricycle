using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class CodecConverterTests
    {
        CodecConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new CodecConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotCodec()
        {
            _converter.Convert("-c", 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertThrowsExceptionWhenValueIsMissingName()
        {
            _converter.Convert("-c", new Codec());
        }

        [TestMethod]
        public void ConvertIncludesName()
        {
            var codec = new Codec("aac");

            Assert.AreEqual("-c aac", _converter.Convert("-c", codec));
        }

        [TestMethod]
        public void ConvertIncludesOptions()
        {
            var codec = new Codec("libx264")
            {
                Options = new Dictionary<string, string>()
                {
                    { "preset", "medium" }
                }
            };

            Assert.AreEqual("-c libx264 -preset medium", _converter.Convert("-c", codec));
        }

        [TestMethod]
        public void ConvertDelimitsOptions()
        {
            var codec = new Codec("libx264")
            {
                Options = new Dictionary<string, string>()
                {
                    { "preset", "medium" },
                    { "crf", "20" }
                }
            };

            Assert.AreEqual("-c libx264 -preset medium -crf 20", _converter.Convert("-c", codec));
        }
    }
}
