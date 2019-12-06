using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class MappedStreamListConverterTests
    {
        MappedStreamListConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new MappedStreamListConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotMappedStreamList()
        {
            _converter.Convert("-map", 0);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueContainsOtherStreamType()
        {
            _converter.Convert("-map", 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertThrowsExceptionWhenValueIsMissingInput()
        {
            _converter.Convert("-map", new MappedStream[] { new MappedStream() });
        }

        [TestMethod]
        public void ConvertIncludesStreamSpecifier()
        {
            var stream = new MappedAudioStream()
            {
                Input = new StreamInput(0, 1)
            };

            Assert.AreEqual("-map 0:1", _converter.Convert("-map", new MappedStream[] { stream }));
        }

        [TestMethod]
        public void ConvertIncludesCodec()
        {
            var stream = new MappedAudioStream()
            {
                Input = new StreamInput(0, 1),
                Codec = new Codec("aac")
            };

            Assert.AreEqual("-map 0:1 -c:a:0 aac", _converter.Convert("-map", new MappedStream[] { stream }));
        }

        [TestMethod]
        public void ConvertIncludesBitrate()
        {
            var stream = new MappedAudioStream()
            {
                Input = new StreamInput(0, 1),
                Bitrate = "160k"
            };

            Assert.AreEqual("-map 0:1 -b:a:0 160k", _converter.Convert("-map", new MappedStream[] { stream }));
        }

        [TestMethod]
        public void ConvertIncludesChannelCount()
        {
            var stream = new MappedAudioStream()
            {
                Input = new StreamInput(0, 1),
                ChannelCount = 2
            };

            Assert.AreEqual("-map 0:1 -ac:a:0 2", _converter.Convert("-map", new MappedStream[] { stream }));
        }

        [TestMethod]
        public void ConvertDelimitsOptions()
        {
            var stream = new MappedAudioStream()
            {
                Input = new StreamInput(0, 1),
                Codec = new Codec("aac"),
                Bitrate = "160k"
            };

            Assert.AreEqual("-map 0:1 -c:a:0 aac -b:a:0 160k", _converter.Convert("-map", new MappedStream[] { stream }));
        }

        [TestMethod]
        public void ConvertTracksSeparateIndexesForStreamTypes()
        {
            var streams = new MappedStream[]
            {
                new MappedVideoStream(new StreamInput(0, 0))
                {
                    Codec = new Codec("libx264")
                },
                new MappedAudioStream(new StreamInput(0, 1))
                {
                    Codec = new Codec("aac")
                },
                new MappedAudioStream(new StreamInput(0, 2))
                {
                    Codec = new Codec("ac3")
                }
            };

            Assert.AreEqual("-map 0:0 -c:v:0 libx264 -map 0:1 -c:a:0 aac -map 0:2 -c:a:1 ac3",
                            _converter.Convert("-map", streams));
        }
    }
}
