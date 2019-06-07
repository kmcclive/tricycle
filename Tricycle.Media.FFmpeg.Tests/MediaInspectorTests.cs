using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Media.Models;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class MediaInspectorTests
    {
        #region Constants

        const string FFPROBE_FILE_NAME = "/usr/sbin/ffprobe";
        const string FILE_NAME_1 = "escaped 1";
        const string FILE_NAME_2 = "escaped 2";
        const string FILE_NAME_3 = "escaped 3";
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
        const int TIMEOUT_MS = 100;

        #endregion

        #region Nested Types

        private class MockProcess : IProcess
        {
            public bool HasExited { get; set; }

            public event Action Exited;
            public event Action<string> ErrorDataReceived;
            public event Action<string> OutputDataReceived;

            public void Dispose()
            {

            }

            public void Kill()
            {
                HasExited = true;
            }

            public bool Start(ProcessStartInfo startInfo)
            {
                if (startInfo == null)
                {
                    throw new ArgumentNullException();
                }

                if (startInfo.FileName != FFPROBE_FILE_NAME)
                {
                    throw new InvalidOperationException();
                }

                const string PATTERN = @"-v\s+quiet\s+-print_format\s+json\s+-show_format\s+-show_streams\s+-i\s+(?<file>.+)";
                var match = Regex.Match(startInfo.Arguments, PATTERN);
                var output = string.Empty;

                switch (match.Groups["file"]?.Value)
                {
                    case FILE_NAME_1:
                        output = FILE_OUTPUT_1;
                        break;
                    case FILE_NAME_2:
                        output = FILE_OUTPUT_2;
                        break;
                    case FILE_NAME_3:
                        Thread.Sleep(TIMEOUT_MS + 5);
                        break;
                    default:
                        if (startInfo.RedirectStandardError)
                        {
                            ErrorDataReceived?.Invoke("Failed to probe file.");
                        }
                        break;
                }

                using (var reader = new StringReader(output))
                {
                    string line = null;

                    do
                    {
                        Thread.Sleep(5);

                        line = reader.ReadLine();

                        if (startInfo.RedirectStandardOutput && (line != null))
                        {
                            OutputDataReceived?.Invoke(line);
                        }
                    } while (line != null);
                }

                HasExited = true;
                Exited?.Invoke();

                return true;
            }
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void TestInspect()
        {
            var process = new MockProcess();
            Func<IProcess> processCreator = () => process;
            var processUtility = Substitute.For<IProcessUtility>();
            var timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS);
            var inspector = new MediaInspector(FFPROBE_FILE_NAME, processCreator, processUtility, timeout);

            #region Test MKV with HDR

            var fileName = "/Users/fred/Documents/video.mkv";

            processUtility.EscapeFilePath(fileName).Returns(FILE_NAME_1);

            MediaInfo info = inspector.Inspect(fileName);

            Assert.IsNotNull(info);
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

            processUtility.EscapeFilePath(fileName).Returns(FILE_NAME_2);

            info = inspector.Inspect(fileName);

            Assert.IsNotNull(info);
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

            #region Test timeout

            var stopwatch = new Stopwatch();

            fileName = "/Users/fred/Documents/complex.m2ts";

            processUtility.EscapeFilePath(fileName).Returns(FILE_NAME_3);
            stopwatch.Start();

            info = inspector.Inspect(fileName);

            stopwatch.Stop();

            Assert.IsTrue(stopwatch.Elapsed > timeout, "The specified timeout was not honored.");

            #endregion

            #region Test bad file

            fileName = "/Users/fred/Documents/text.txt";

            processUtility.EscapeFilePath(fileName).Returns("bad file");

            info = inspector.Inspect(fileName);

            Assert.IsNull(info);

            #endregion
        }

        #endregion
    }
}
