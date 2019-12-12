using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

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
            _converter.Reflector = Substitute.For<IArgumentPropertyReflector>();

            var mockConverter = Substitute.For<IArgumentConverter>();

            mockConverter.Convert(Arg.Any<string>(), Arg.Any<object>()).Returns(x => $"{x[0]} {x[1]}");

            _converter.Reflector.Reflect(Arg.Is<object>(x => x is MappedStream)).Returns(x =>
            {
                var stream = x[0] as MappedStream;
                IList<ArgumentProperty> result = new List<ArgumentProperty>();

                if (stream.Codec != null)
                {
                    result.Add(new ArgumentProperty()
                    {
                        PropertyName = nameof(stream.Codec),
                        ArgumentName = "-c",
                        Value = stream.Codec,
                        Converter = mockConverter
                    });
                }

                if (stream.Bitrate != null)
                {
                    result.Add(new ArgumentProperty()
                    {
                        PropertyName = nameof(stream.Bitrate),
                        ArgumentName = "-b",
                        Value = stream.Bitrate,
                        Converter = mockConverter
                    });
                }

                if (stream is MappedAudioStream audioStream && audioStream.ChannelCount.HasValue)
                {
                    result.Add(new ArgumentProperty()
                    {
                        PropertyName = nameof(audioStream.ChannelCount),
                        ArgumentName = "-ac",
                        Value = audioStream.ChannelCount,
                        Converter = mockConverter
                    });
                }

                return result;
            });

            _converter.Reflector.Reflect(Arg.Is<object>(x => x is Codec)).Returns(x =>
            {
                var codec = x[0] as Codec;
                IList<ArgumentProperty> result = new List<ArgumentProperty>();

                if (codec.Name != null)
                {
                    result.Add(new ArgumentProperty()
                    {
                        PropertyName = nameof(codec.Name),
                        Value = codec.Name,
                        Converter = mockConverter
                    });
                }

                return result;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotMappedStreamList()
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
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueContainsOtherStreamType()
        {
            _converter.Convert("-map", new MappedStream[] { new MappedStream(StreamType.Other, new StreamInput(0, 0)) });
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
