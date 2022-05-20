using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests;

[TestClass]
public class CropDetectorTests
{
    [TestMethod]
    public async Task TestDetect()
    {
        const string ARGS = "generated arguments";

        var processRunner = Substitute.For<IProcessRunner>();
        var timeout = TimeSpan.FromMilliseconds(10);
        var ffmpegFileName = "/usr/sbin/ffmpeg";
        var config = new FFmpegConfig();
        var configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
        var argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
        var detector = new CropDetector(ffmpegFileName, processRunner, configManager, argumentGenerator, timeout);
        IList<FFmpegJob> jobs = new List<FFmpegJob>();

        configManager.Config = config;
        argumentGenerator.GenerateArguments(Arg.Any<FFmpegJob>()).Returns(ARGS);
        argumentGenerator.When(x => x.GenerateArguments(Arg.Any<FFmpegJob>()))
                         .Do(x => jobs.Add(x[0] as FFmpegJob));

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
        var outputs = new string[]
        {
            "[Parsed_cropdetect_0 @ 0x7fce49600000] x1:0 x2:3839 y1:277 y2:1882 w:3840 h:1600 x:0 y:280 pts:102 t:0.102000 crop=3840:1600:0:280",
            "[Parsed_cropdetect_0 @ 0x7f8bf045da80] x1:859 x2:3839 y1:277 y2:1882 w:2976 h:1600 x:862 y:280 pts:120 t:0.120000 crop=2976:1600:862:280",
            "[Parsed_cropdetect_0 @ 0x7f9437704a00] x1:0 x2:3821 y1:277 y2:1774 w:3808 h:1488 x:8 y:282 pts:97 t:0.097000 crop=3808:1488:8:282",
            "[Parsed_cropdetect_0 @ 0x7f9032448880] x1:0 x2:3423 y1:277 y2:1882 w:3424 h:1600 x:0 y:280 pts:115 t:0.115000 crop=3424:1600:0:280",
            "[Parsed_cropdetect_0 @ 0x7ff79975be00] x1:1055 x2:3839 y1:277 y2:1882 w:2784 h:1600 x:1056 y:280 pts:91 t:0.091000 crop=2784:1600:1056:280"
        };
        int i = 0;

        processRunner.Run(ffmpegFileName, ARGS, timeout)
                     .Returns(new ProcessResult() { ErrorData = outputs[i++] });

        CropParameters parameters = await detector.Detect(mediaInfo);

        Assert.AreEqual(5, jobs.Count);

        for (i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];

            Assert.IsNotNull(job);
            Assert.IsTrue(job.HideBanner);
            Assert.AreEqual(2, job.FrameCount);
            Assert.AreEqual(mediaInfo.FileName, job.InputFileName);
            Assert.AreEqual(TimeSpan.FromMinutes(i + 1), job.StartTime);
            Assert.AreEqual("cropdetect", (job.Filters.FirstOrDefault() as CustomFilter)?.Data);
        }

        Assert.IsNotNull(parameters);
        Assert.AreEqual(new Coordinate<int>(0, 280), parameters.Start);
        Assert.AreEqual(new Dimensions(3840, 1600), parameters.Size);

        #endregion

        #region Test video that does not have bars

        mediaInfo = new MediaInfo()
        {
            FileName = "/Users/fred/Documents/video2.mkv",
            Duration = TimeSpan.FromMinutes(1)
        };
        var output = @"
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

        processRunner.Run(ffmpegFileName, ARGS, timeout)
                     .Returns(new ProcessResult() { ErrorData = output });
        jobs.Clear();

        parameters = await detector.Detect(mediaInfo);

        Assert.AreEqual(5, jobs.Count);

        for (i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];

            Assert.AreEqual(TimeSpan.FromSeconds((i + 1) * 6), job?.StartTime);
        }

        Assert.IsNotNull(parameters);
        Assert.AreEqual(new Coordinate<int>(0, 0), parameters.Start);
        Assert.AreEqual(new Dimensions(3840, 2160), parameters.Size);

        #endregion

        #region Test that config is used

        config.Video = new VideoConfig()
        {
            CropDetectOptions = "24:16:0"
        };

        processRunner.Run(ffmpegFileName, ARGS, timeout)
                     .Returns(new ProcessResult() { ErrorData = output });
        jobs.Clear();
        parameters = await detector.Detect(mediaInfo);

        foreach (var job in jobs)
        {
            Assert.AreEqual($"cropdetect={config.Video.CropDetectOptions}",
                            (job?.Filters?.FirstOrDefault() as CustomFilter)?.Data);
        }

        #endregion
    }
}
