using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.Diagnostics.Utilities;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class CropDetectorTests
    {
        [TestMethod]
        public async Task TestDetect()
        {
            const string ARG_PATTERN = "-hide_banner\\s+-ss\\s+{0}\\s+-i\\s+{1}\\s+-frames:vf\\s+2\\s+-vf\\s+cropdetect(=\\d+:\\d+:\\d+)?\\s+-f\\s+null\\s+-";

            var processRunner = Substitute.For<IProcessRunner>();
            var processUtility = Substitute.For<IProcessUtility>();
            var timeout = TimeSpan.FromMilliseconds(10);
            var ffmpegFileName = "/usr/sbin/ffmpeg";
            var detector = new CropDetector(ffmpegFileName, processRunner, processUtility, timeout);

            #region Test Exceptions

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await detector.Detect(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await detector.Detect(new MediaInfo() { FileName = "test" }));

            #endregion

            #region Test video that has bars and exceeds max seek time

            var mediaInfo = new MediaInfo()
            {
                FileName = "/Users/fred/Documents/video.mkv",
                Duration = TimeSpan.FromMinutes(104)
            };
            var escapedFileName = "escaped file";
            var output = @"
                Stream #0:0(eng): Video: hevc (Main 10), yuv420p10le(tv, bt2020nc/bt2020/smpte2084), 3840x2160 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn, 23.98 tbc
                Metadata:
                    BPS-eng         : 75334576
                    DURATION-eng    : 01:44:06.114875000
                    NUMBER_OF_FRAMES-eng: 149757
                    NUMBER_OF_BYTES-eng: 58818544507
                    SOURCE_ID-eng   : 001011
                    _STATISTICS_WRITING_DATE_UTC-eng: 2019-05-25 18:22:35
                    _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES SOURCE_ID
                [Parsed_cropdetect_0 @ 0x7faf2dc72f00] x1:0 x2:3839 y1:263 y2:1896 w:3840 h:1632 x:0 y:264 pts:91 t:0.091000 crop=3840:1632:0:264
                frame=    2 fps=0.0 q=-0.0 Lsize=N/A time=00:00:00.74 bitrate=N/A speed=0.802x";

            processUtility.EscapeFilePath(mediaInfo.FileName).Returns(escapedFileName);
            processRunner.Run(ffmpegFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, string.Format(ARG_PATTERN, 300, escapedFileName))),
                              timeout)
                         .Returns(new ProcessResult() { ErrorData = output });

            CropParameters parameters = await detector.Detect(mediaInfo);

            Assert.IsNotNull(parameters);
            Assert.AreEqual(new Coordinate<int>(0, 264), parameters.Start);
            Assert.AreEqual(new Dimensions(3840, 1632), parameters.Size);

            #endregion

            #region Test video that does not have bars

            mediaInfo = new MediaInfo()
            {
                FileName = "/Users/fred/Documents/video2.mkv",
                Duration = TimeSpan.FromMinutes(1)
            };
            escapedFileName = "escaped file 2";
            output = @"
                Stream #0:0(eng): Video: hevc (Main 10), yuv420p10le(tv, bt2020nc/bt2020/smpte2084), 3840x2160 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn, 23.98 tbc
                Metadata:
                  BPS-eng         : 42940118
                  DURATION-eng    : 00:01:00
                  NUMBER_OF_FRAMES-eng: 75098
                  NUMBER_OF_BYTES-eng: 16812194126
                  SOURCE_ID-eng   : 001011
                  _STATISTICS_WRITING_DATE_UTC-eng: 2019-06-06 00:00:24
                  _STATISTICS_TAGS-eng: BPS DURATION NUMBER_OF_FRAMES NUMBER_OF_BYTES SOURCE_ID
                [Parsed_cropdetect_0 @ 0x7ff6cce00a80] x1:0 x2:3839 y1:0 y2:2159 w:3840 h:2160 x:0 y:0 pts:91 t:0.091000 crop=3840:2160:0:0
                frame=    2 fps=0.0 q=-0.0 Lsize=N/A time=00:00:00.63 bitrate=N/A speed=1.87x";

            processUtility.EscapeFilePath(mediaInfo.FileName).Returns(escapedFileName);
            processRunner.Run(ffmpegFileName,
                              Arg.Is<string>(s => Regex.IsMatch(s, string.Format(ARG_PATTERN, 30, escapedFileName))),
                              timeout)
                         .Returns(new ProcessResult() { ErrorData = output });

            parameters = await detector.Detect(mediaInfo);

            Assert.IsNotNull(parameters);
            Assert.AreEqual(new Coordinate<int>(0, 0), parameters.Start);
            Assert.AreEqual(new Dimensions(3840, 2160), parameters.Size);

            #endregion
        }
    }
}
