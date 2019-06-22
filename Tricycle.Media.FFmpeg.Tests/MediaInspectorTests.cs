using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Media.Models;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class MediaInspectorTests
    {
        #region Constants

        const string FILE_OUTPUT_1 =
            @"{
                ""streams"": [
                    {
                        ""index"": 0,
                        ""codec_name"": ""hevc"",
                        ""codec_long_name"": ""H.265 / HEVC (High Efficiency Video Coding)"",
                        ""codec_type"": ""video"",
                        ""width"": 3840,
                        ""height"": 2160,
                        ""pix_fmt"": ""yuv420p10le"",
                        ""color_transfer"": ""smpte2084"",
                        ""tags"": {
                            ""language"": ""eng"",
                        }
                    },
                    {
                        ""index"": 1,
                        ""codec_name"": ""dts"",
                        ""codec_long_name"": ""DCA (DTS Coherent Acoustics)"",
                        ""codec_type"": ""audio"",
                        ""channels"": 6,
                        ""tags"": {
                            ""language"": ""eng"",
                        }
                    },
                    {
                        ""index"": 2,
                        ""codec_name"": ""dts"",
                        ""codec_long_name"": ""DCA (DTS Coherent Acoustics)"",
                        ""codec_type"": ""audio"",
                        ""channels"": 6,
                        ""tags"": {
                            ""language"": ""eng"",
                        }
                    },
                    {
                        ""index"": 3,
                        ""codec_name"": ""hdmv_pgs_subtitle"",
                        ""codec_long_name"": ""HDMV Presentation Graphic Stream subtitles"",
                        ""codec_type"": ""subtitle"",
                        ""tags"": {
                            ""language"": ""eng"",
                        }
                    }
                ],
                ""format"": {
                    ""format_name"": ""matroska,webm"",
                    ""format_long_name"": ""Matroska / WebM"",
                    ""duration"": ""3186.808000"",
                }
            }";
        const string FILE_OUTPUT_2 =
            @"{
                ""streams"": [
                    {
                        ""index"": 0,
                        ""codec_name"": ""h264"",
                        ""codec_long_name"": ""H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10"",
                        ""codec_type"": ""video"",
                        ""width"": 1920,
                        ""height"": 1080,
                        ""pix_fmt"": ""yuv420p"",
                        ""color_transfer"": ""bt709"",
                        ""tags"": {
                            ""language"": ""und"",
                        }
                    },
                    {
                        ""index"": 1,
                        ""codec_name"": ""aac"",
                        ""codec_long_name"": ""AAC (Advanced Audio Coding)"",
                        ""codec_type"": ""audio"",
                        ""channels"": 2,
                        ""tags"": {
                            ""language"": ""eng"",
                        }
                    },
                    {
                        ""index"": 2,
                        ""codec_name"": ""ac3"",
                        ""codec_long_name"": ""ATSC A/52A (AC-3)"",
                        ""codec_type"": ""audio"",
                        ""channels"": 6,
                        ""tags"": {
                            ""language"": ""eng"",
                        },
                    },
                ],
                ""format"": {
                    ""format_name"": ""mov,mp4,m4a,3gp,3g2,mj2"",
                    ""format_long_name"": ""QuickTime / MOV"",
                    ""duration"": ""1597.654000"",
                }
            }";
        const string FRAME_OUTPUT =
            @"{
                ""frames"": [
                    {
                        ""media_type"": ""video"",
                        ""stream_index"": 0,
                        ""key_frame"": 1,
                        ""width"": 3840,
                        ""height"": 2160,
                        ""color_transfer"": ""smpte2084"",
                        ""side_data_list"": [
                            {
                                ""side_data_type"": ""Mastering display metadata"",
                                ""red_x"": ""34000/50000"",
                                ""red_y"": ""16000/50000"",
                                ""green_x"": ""13250/50000"",
                                ""green_y"": ""34500/50000"",
                                ""blue_x"": ""7500/50000"",
                                ""blue_y"": ""3000/50000"",
                                ""white_point_x"": ""15635/50000"",
                                ""white_point_y"": ""16450/50000"",
                                ""min_luminance"": ""1/10000"",
                                ""max_luminance"": ""10000000/10000""
                            },
                            {
                                ""side_data_type"": ""Content light level metadata"",
                                ""max_content"": 1000,
                                ""max_average"": 400
                            }
                        ]
                    }
                ]
            }";

        #endregion

        #region Test Methods

        [TestMethod]
        public void TestInspect()
        {
            var processRunner = Substitute.For<IProcessRunner>();
            var processUtility = Substitute.For<IProcessUtility>();
            var timeout = TimeSpan.FromMilliseconds(2);
            var ffprobeFileName = "/usr/sbin/ffprobe";
            var inspector = new MediaInspector(ffprobeFileName, processRunner, processUtility, timeout);

            #region Test MKV with HDR

            var fileName = "/Users/fred/Documents/video.mkv";
            var escapedFileName = "escaped 1";
            var argPattern1 = @"-loglevel\s+error\s+-print_format\s+json\s+-show_format\s+-show_streams\s+-i\s+";
            var argPattern2 = @"-loglevel\s+error\s+-print_format\s+json\s+-show_frames\s+-select_streams\s+0\s+-read_intervals\s+%\+#1\s+-i\s+";

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = FILE_OUTPUT_1 });
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern2 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = FRAME_OUTPUT });

            MediaInfo info = inspector.Inspect(fileName);

            Assert.IsNotNull(info);
            Assert.AreEqual(fileName, info.FileName);
            Assert.AreEqual("Matroska / WebM", info.FormatName);
            Assert.AreEqual(TimeSpan.FromSeconds(3186.808000), info.Duration);

            Assert.IsNotNull(info.Streams);
            Assert.AreEqual(4, info.Streams.Count);

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
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[2];

            Assert.IsNotNull(stream);
            Assert.AreEqual("dts", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(2, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[3];

            Assert.IsNotNull(stream);
            Assert.AreEqual("hdmv_pgs_subtitle", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(3, stream.Index);
            Assert.AreEqual(StreamType.Subtitle, stream.StreamType);

            #endregion

            #region Test MP4 with SDR

            fileName = "/Users/fred/Documents/video.m4v";
            escapedFileName = "escaped 2";

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult() { OutputData = FILE_OUTPUT_2 });

            info = inspector.Inspect(fileName);

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
            Assert.AreEqual(2, ((AudioStreamInfo)stream).ChannelCount);

            stream = info.Streams[2];

            Assert.IsNotNull(stream);
            Assert.AreEqual("ac3", stream.FormatName);
            Assert.AreEqual("eng", stream.Language);
            Assert.AreEqual(2, stream.Index);
            Assert.IsInstanceOfType(stream, typeof(AudioStreamInfo));
            Assert.AreEqual(6, ((AudioStreamInfo)stream).ChannelCount);

            #endregion

            #region Test bad file

            fileName = "/Users/fred/Documents/text.txt";
            escapedFileName = "bad file";

            processUtility.EscapeFilePath(fileName).Returns(escapedFileName);
            processRunner.Run(ffprobeFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, argPattern1 + escapedFileName)),
                              timeout)
                         .Returns(new ProcessResult());

            info = inspector.Inspect(fileName);

            Assert.IsNull(info);

            #endregion
        }

        #endregion
    }
}
