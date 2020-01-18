using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class InterlaceDetectorTests
    {
        InterlaceDetector _detector;
        string _ffmpegFileName;
        IProcessRunner _processRunner;
        IFFmpegArgumentGenerator _argumentGenerator;
        TimeSpan _timeout;
        MediaInfo _mediaInfo;

        [TestInitialize]
        public void Setup()
        {
            _ffmpegFileName = "/usr/sbin/ffmpeg";
            _processRunner = Substitute.For<IProcessRunner>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _timeout = TimeSpan.FromMilliseconds(100);
            _detector = new InterlaceDetector(_ffmpegFileName, _processRunner, _argumentGenerator, _timeout);

            _mediaInfo = new MediaInfo()
            {
                FileName = "/Users/fred/Movies/movie.mkv",
                Duration = TimeSpan.FromHours(2)
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectThrowsExceptionWhenMediaInfoIsNull()
        {
            await _detector.Detect(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DetectThrowsExceptionWhenFileNameIsNull()
        {
            _mediaInfo.FileName = null;

            await _detector.Detect(_mediaInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DetectThrowsExceptionWhenFileNameIsEmpty()
        {
            _mediaInfo.FileName = string.Empty;

            await _detector.Detect(_mediaInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DetectThrowsExceptionWhenDurationIsZero()
        {
            _mediaInfo.Duration = TimeSpan.Zero;

            await _detector.Detect(_mediaInfo);
        }

        [TestMethod]
        public async Task DetectGeneratesArguments()
        {
            FFmpegJob job = null;

            _argumentGenerator.When(x => x.GenerateArguments(Arg.Any<FFmpegJob>()))
                              .Do(x => job = x[0] as FFmpegJob);

            await _detector.Detect(_mediaInfo);

            Assert.IsNotNull(job);
            Assert.AreEqual(TimeSpan.FromHours(1), job.StartTime);
            Assert.AreEqual(_mediaInfo.FileName, job.InputFileName);
            Assert.AreEqual(100, job.FrameCount);
            Assert.AreEqual(1, job.Filters?.Count);
            Assert.AreEqual("idet", (job.Filters[0] as Filter).Name);
        }

        [TestMethod]
        public async Task DetectRunsProcess()
        {
            string arguments = "generated";

            _argumentGenerator.GenerateArguments(Arg.Any<FFmpegJob>()).Returns(arguments);

            await _detector.Detect(_mediaInfo);

            await _processRunner.Received().Run(_ffmpegFileName, arguments, _timeout);
        }

        [TestMethod]
        public async Task DetectReturnsTrueWhenMediaIsInterlaced()
        {
            string output =
                @"[Parsed_idet_0 @ 0x7fd17c62c980] Repeated Fields: Neither:     0 Top:     0 Bottom:     0
                  [Parsed_idet_0 @ 0x7fd17c62c980] Single frame detection: TFF:     0 BFF:     0 Progressive:     0 Undetermined:     0
                  [Parsed_idet_0 @ 0x7fd17c62c980] Multi frame detection: TFF:     0 BFF:     0 Progressive:     0 Undetermined:     0
                  frame=  100 fps=0.0 q=-0.0 Lsize=N/A time=00:00:03.39 bitrate=N/A speed=49.5x
                  [Parsed_idet_0 @ 0x7fd17c40ec40] Repeated Fields: Neither:    96 Top:     4 Bottom:     1
                  [Parsed_idet_0 @ 0x7fd17c40ec40] Single frame detection: TFF:    62 BFF:     0 Progressive:     8 Undetermined:    31
                  [Parsed_idet_0 @ 0x7fd17c40ec40] Multi frame detection: TFF:   101 BFF:     0 Progressive:     0 Undetermined:     0";

            _processRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>())
                          .Returns(new ProcessResult() { ErrorData = output });

            Assert.IsTrue(await _detector.Detect(_mediaInfo));
        }

        [TestMethod]
        public async Task DetectReturnsFalseWhenMediaIsNotInterlaced()
        {
            string output =
                @"[Parsed_idet_0 @ 0x7fd17c62c980] Repeated Fields: Neither:     0 Top:     0 Bottom:     0
                  [Parsed_idet_0 @ 0x7fd17c62c980] Single frame detection: TFF:     0 BFF:     0 Progressive:     0 Undetermined:     0
                  [Parsed_idet_0 @ 0x7fd17c62c980] Multi frame detection: TFF:     0 BFF:     0 Progressive:     0 Undetermined:     0
                  frame=  100 fps=0.0 q=-0.0 Lsize=N/A time=00:00:04.18 bitrate=N/A speed=51.2x
                  [Parsed_idet_0 @ 0x7faa0f605340] Repeated Fields: Neither:   101 Top:     0 Bottom:     0
                  [Parsed_idet_0 @ 0x7faa0f605340] Single frame detection: TFF:     0 BFF:     0 Progressive:    43 Undetermined:    58
                  [Parsed_idet_0 @ 0x7faa0f605340] Multi frame detection: TFF:     0 BFF:     0 Progressive:   100 Undetermined:     1";

            _processRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>())
                          .Returns(new ProcessResult() { ErrorData = output });

            Assert.IsFalse(await _detector.Detect(_mediaInfo));
        }
    }
}
