using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.FFprobe;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class MediaInspectorTests
    {
        #region Constants

        static readonly Output FILE_OUTPUT_1 = new Output()
        {
            Streams = new Stream[]
            {
                new Stream()
                {
                    Index = 0,
                    CodecName = "hevc",
                    CodecLongName = "H.265 / HEVC (High Efficiency Video Coding)",
                    CodecType = "video",
                    Width = 3840,
                    Height = 2160,
                    PixFmt = "yuv420p10le",
                    ColorTransfer = "smpte2084",
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                },
                new Stream()
                {
                    Index = 1,
                    CodecName = "dts",
                    CodecLongName = "DCA (DTS Coherent Acoustics)",
                    CodecType = "audio",
                    Channels = 6,
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                },
                new Stream()
                {
                    Index = 2,
                    CodecName = "dts",
                    CodecLongName = "DCA (DTS Coherent Acoustics)",
                    CodecType = "audio",
                    Channels = 6,
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                },
                new Stream()
                {
                    Index = 3,
                    CodecName = "hdmv_pgs_subtitle",
                    CodecLongName = "HDMV Presentation Graphic Stream subtitles",
                    CodecType = "subtitle",
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                },
                new Stream()
                {
                    Index = 4,
                    CodecName = "subrip",
                    CodecLongName = "SubRip subtitle",
                    CodecType = "subtitle",
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                }
            },
            Format = new Format()
            {
                FormatName = "matroska,webm",
                FormatLongName = "Matroska / WebM",
                Duration = "3186.808000"
            }
        };
        static readonly Output FILE_OUTPUT_2 = new Output()
        {
            Streams = new Stream[]
            {
                new Stream()
                {
                    Index = 0,
                    CodecName = "h264",
                    CodecLongName = "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
                    CodecType = "video",
                    Width = 1920,
                    Height = 1080,
                    PixFmt = "yuv420p",
                    ColorTransfer = "bt709",
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "und" }
                    }
                },
                new Stream()
                {
                    Index = 1,
                    CodecName = "aac",
                    CodecLongName = "AAC (Advanced Audio Coding)",
                    CodecType = "audio",
                    Channels = 2,
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                },
                new Stream()
                {
                    Index = 2,
                    CodecName = "ac3",
                    CodecLongName = "ATSC A/52A (AC-3)",
                    CodecType = "audio",
                    Channels = 6,
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                }
            },
            Format = new Format()
            {
                FormatName = "mov,mp4,m4a,3gp,3g2,mj2",
                FormatLongName = "QuickTime / MOV",
                Duration = "1597.654000"
            }
        };
        static readonly Output FILE_OUTPUT_3 = new Output()
        {
            Streams = new Stream[]
            {
                new Stream()
                {
                    Index = 0,
                    CodecName = "mpeg2video",
                    CodecLongName = "MPEG-2 video",
                    CodecType = "video",
                    Width = 720,
                    Height = 480,
                    SampleAspectRatio = "8:9",
                    PixFmt = "yuv420p",
                    ColorTransfer = "smpte170m",
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "und" }
                    }
                },
                new Stream()
                {
                    Index = 1,
                    CodecName = "ac3",
                    CodecLongName = "ATSC A/52A (AC-3)",
                    CodecType = "audio",
                    Channels = 2,
                    Tags = new Dictionary<string, string>()
                    {
                        { "language", "eng" }
                    }
                }
            },
            Format = new Format()
            {
                FormatName = "mov,mp4,m4a,3gp,3g2,mj2",
                FormatLongName = "QuickTime / MOV",
                Duration = "1547.168000"
            }
        };
        static readonly FrameOutput FRAME_OUTPUT = new FrameOutput()
        {
            Frames = new Frame[]
            {
                new Frame()
                {
                    MediaType = "video",
                    StreamIndex = 0,
                    KeyFrame = 1,
                    Width = 3840,
                    Height = 2160,
                    ColorTransfer = "smpte2084",
                    SideDataList = new SideData[]
                    {
                        new SideData()
                        {
                            SideDataType = "Mastering display metadata",
                            RedX = "34000/50000",
                            RedY = "16000/50000",
                            GreenX = "13250/50000",
                            GreenY = "34500/50000",
                            BlueX = "7500/50000",
                            BlueY = "3000/50000",
                            WhitePointX = "15635/50000",
                            WhitePointY = "16450/50000",
                            MaxLuminance = "10000000/10000"
                        },
                        new SideData()
                        {
                            SideDataType = "Content light level metadata",
                            MaxContent = 1000,
                            MaxAverage = 400
                        }
                    }
                }
            }
        };

        #endregion

        #region Test Methods

        [TestMethod]
        public async Task TestInspect()
        {
            var processRunner = Substitute.For<IProcessRunner>();
            var processUtility = Substitute.For<IProcessUtility>();
            var serializer = Substitute.For<ISerializer<string>>();
            var timeout = TimeSpan.FromMilliseconds(10);
            var ffprobeFileName = "/usr/sbin/ffprobe";
            var inspector = new MediaInspector(ffprobeFileName, processRunner, processUtility, serializer, timeout);

            #region Test Exceptions

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await inspector.Inspect(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await inspector.Inspect(string.Empty));

            #endregion

            #region Test MKV with HDR

            var fileName = "/Users/fred/Documents/video.mkv";
            var escapedFileName = "escaped 1";
            var argPattern1 = @"-loglevel\s+error\s+-print_format\s+json\s+-show_format\s+-show_streams\s+-i\s+";
            var argPattern2 = @"-loglevel\s+error\s+-print_format\s+json\s+-show_frames\s+-select_streams\s+0\s+-read_intervals\s+%\+#1\s+-i\s+";
            var outputText = Guid.NewGuid().ToString();
            var frameText = Guid.NewGuid().ToString();

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = outputText });
            serializer.Deserialize<Output>(outputText).Returns(FILE_OUTPUT_1);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern2 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = frameText });
            serializer.Deserialize<FrameOutput>(frameText).Returns(FRAME_OUTPUT);

            MediaInfo info = await inspector.Inspect(fileName);

            Assert.IsNotNull(info);
            Assert.AreEqual(fileName, info.FileName);
            Assert.AreEqual("Matroska / WebM", info.FormatName);
            Assert.AreEqual(TimeSpan.FromSeconds(3186.808000), info.Duration);

            Assert.IsNotNull(info.Streams);
            Assert.AreEqual(5, info.Streams.Count);

            StreamInfo stream = info.Streams[0];

            Assert.IsNotNull(stream);
            Assert.AreEqual("hevc", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(0, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(VideoStreamInfo));

            var videoStream = (VideoStreamInfo)stream;

            Assert.AreEqual(new Dimensions(3840, 2160), videoStream.Dimensions);
            Assert.AreEqual(DynamicRange.High, videoStream.DynamicRange);
            Assert.AreEqual(10, videoStream.BitDepth);

            var displayProperties = videoStream.MasterDisplayProperties;

            Assert.IsNotNull(displayProperties);
            Assert.AreEqual(34000, displayProperties.Red.X);
            Assert.AreEqual(16000, displayProperties.Red.Y);
            Assert.AreEqual(13250, displayProperties.Green.X);
            Assert.AreEqual(34500, displayProperties.Green.Y);
            Assert.AreEqual(7500, displayProperties.Blue.X);
            Assert.AreEqual(3000, displayProperties.Blue.Y);
            Assert.AreEqual(15635, displayProperties.WhitePoint.X);
            Assert.AreEqual(16450, displayProperties.WhitePoint.Y);

            var lightProperties = videoStream.LightLevelProperties;

            Assert.IsNotNull(lightProperties);
            Assert.AreEqual(1000, lightProperties.MaxCll);
            Assert.AreEqual(400, lightProperties.MaxFall);

            stream = info.Streams[1];

            Assert.IsNotNull(stream);
            Assert.AreEqual("dts", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(1, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(AudioFormat.Dts, ((AudioStreamInfo)stream).Format);
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[2];

            Assert.IsNotNull(stream);
            Assert.AreEqual("dts", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(2, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(AudioFormat.Dts, ((AudioStreamInfo)stream).Format);
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[3];

            Assert.IsNotNull(stream);
            Assert.AreEqual("hdmv_pgs_subtitle", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(3, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(SubtitleStreamInfo));
            Assert.AreEqual(SubtitleType.Graphic, ((SubtitleStreamInfo)stream).SubtitleType);

            stream = info.Streams[4];

            Assert.IsNotNull(stream);
            Assert.AreEqual("subrip", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(4, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(SubtitleStreamInfo));
            Assert.AreEqual(SubtitleType.Text, ((SubtitleStreamInfo)stream).SubtitleType);

            #endregion

            #region Test MP4 with SDR

            fileName = "/Users/fred/Documents/video.m4v";
            escapedFileName = "escaped 2";
            outputText = Guid.NewGuid().ToString();

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = outputText });
            serializer.Deserialize<Output>(outputText).Returns(FILE_OUTPUT_2);

            info = await inspector.Inspect(fileName);

            Assert.IsNotNull(info);
            Assert.AreEqual(fileName, info.FileName);
            Assert.AreEqual("QuickTime / MOV", info.FormatName);
            Assert.AreEqual(TimeSpan.FromSeconds(1597.654000), info.Duration);

            Assert.IsNotNull(info.Streams);
            Assert.AreEqual(3, info.Streams.Count);

            stream = info.Streams[0];

            Assert.IsNotNull(stream);
            Assert.AreEqual("h264", stream.FormatName);
            Assert.AreEqual("und", stream.Language);
            Assert.AreEqual(0, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(VideoStreamInfo));

            videoStream = (VideoStreamInfo)stream;

            Assert.AreEqual(new Dimensions(1920, 1080), videoStream.Dimensions);
            Assert.AreEqual(DynamicRange.Standard, videoStream.DynamicRange);
            Assert.AreEqual(8, videoStream.BitDepth);
            Assert.IsNull(videoStream.MasterDisplayProperties);
            Assert.IsNull(videoStream.LightLevelProperties);

            stream = info.Streams[1];

            Assert.IsNotNull(stream);
            Assert.AreEqual("aac", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(1, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(AudioFormat.Aac, ((AudioStreamInfo)stream).Format);
            Assert.AreEqual(2, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[2];

            Assert.IsNotNull(stream);
            Assert.AreEqual("ac3", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(2, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(AudioFormat.Ac3, ((AudioStreamInfo)stream).Format);
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            #endregion

            #region Test MP4 with anamorphic video

            fileName = "/Users/fred/Documents/anamorphic.m4v";
            escapedFileName = "escaped 3";
            outputText = Guid.NewGuid().ToString();

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = outputText });
            serializer.Deserialize<Output>(outputText).Returns(FILE_OUTPUT_3);

            info = await inspector.Inspect(fileName);

            Assert.IsNotNull(info);
            Assert.AreEqual(fileName, info.FileName);
            Assert.AreEqual("QuickTime / MOV", info.FormatName);
            Assert.AreEqual(TimeSpan.FromSeconds(1547.168000), info.Duration);

            Assert.IsNotNull(info.Streams);
            Assert.AreEqual(2, info.Streams.Count);

            stream = info.Streams[0];

            Assert.IsNotNull(stream);
            Assert.AreEqual("mpeg2video", stream.FormatName);
            Assert.AreEqual("und", stream.Language);
            Assert.AreEqual(0, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(VideoStreamInfo));

            videoStream = (VideoStreamInfo)stream;

            Assert.AreEqual(new Dimensions(640, 480), videoStream.Dimensions);
            Assert.AreEqual(new Dimensions(720, 480), videoStream.StorageDimensions);
            Assert.AreEqual(DynamicRange.Standard, videoStream.DynamicRange);
            Assert.AreEqual(8, videoStream.BitDepth);
            Assert.IsNull(videoStream.MasterDisplayProperties);
            Assert.IsNull(videoStream.LightLevelProperties);

            stream = info.Streams[1];

            Assert.IsNotNull(stream);
            Assert.AreEqual("ac3", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(1, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(AudioFormat.Ac3, ((AudioStreamInfo)stream).Format);
            Assert.AreEqual(2, ((AudioStreamInfo)stream).ChannelCount);

            #endregion

            #region Test bad file

            fileName = "/Users/fred/Documents/text.txt";
            escapedFileName = "bad file";

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult());

            info = await inspector.Inspect(fileName);

            Assert.IsNull(info);

            #endregion
        }

        #endregion
    }
}
