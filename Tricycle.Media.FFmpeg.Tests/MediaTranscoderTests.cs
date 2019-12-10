using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class MediaTranscoderTests
    {
        string _ffmpegFileName;
        IProcess _process;
        IConfigManager<FFmpegConfig> _configManager;
        IFFmpegArgumentGenerator _argumentGenerator;
        MediaTranscoder _transcoder;
        VideoStreamInfo _videoSource;
        VideoOutputStream _videoOutput;
        TranscodeJob _transcodeJob;

        [TestInitialize]
        public void Setup()
        {
            _ffmpegFileName = "usr/sbin/ffmpeg";
            _process = Substitute.For<IProcess>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _transcoder = new MediaTranscoder(_ffmpegFileName, () => _process, _configManager, _argumentGenerator);

            _videoSource = new VideoStreamInfo()
            {
                Index = 0
            };
            _videoOutput = new VideoOutputStream()
            {
                SourceStreamIndex = 0
            };
            _transcodeJob = new TranscodeJob()
            {
                SourceInfo = new MediaInfo()
                {
                    FileName = "source",
                    Streams = new List<StreamInfo>()
                    {
                        _videoSource
                    }
                },
                OutputFileName = "destination",
                Streams = new List<OutputStream>()
                {
                    _videoOutput
                }
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartThrowsForNullArgument()
        {
            _transcoder.Start(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartThrowsForRunningJob()
        {
            _transcoder.Start(_transcodeJob);

            _process.HasExited.Returns(false);

            _transcoder.Start(_transcodeJob);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StopThrowsWhenNoJobStarted()
        {
            _transcoder.Stop();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StopThrowsWhenNoJobRunning()
        {
            _transcoder.Start(_transcodeJob);

            _process.HasExited.Returns(true);

            _transcoder.Stop();
        }

        [TestMethod]
        public void IsRunningReturnsFalseWhenNoJobStarted()
        {
            Assert.IsFalse(_transcoder.IsRunning);
        }

        [TestMethod]
        public void IsRunningReturnsFalseWhenNoJobRunning()
        {
            _transcoder.Start(_transcodeJob);

            _process.HasExited.Returns(true);
            
            Assert.IsFalse(_transcoder.IsRunning);
        }

        [TestMethod]
        public void IsRunningReturnsTrueForRunningJob()
        {
            _transcoder.Start(_transcodeJob);

            _process.HasExited.Returns(false);

            Assert.IsTrue(_transcoder.IsRunning);
        }

        [TestMethod]
        public void StartCallsProcessStart()
        {
            _transcoder.Start(_transcodeJob);

            _process.Received().Start(Arg.Any<ProcessStartInfo>());
        }

        [TestMethod]
        public void StartPassesCorrectProcessStartInfo()
        {
            string arguments = "transcode arguments";
            ProcessStartInfo startInfo = null;

            _argumentGenerator.GenerateArguments(Arg.Any<FFmpegJob>()).Returns(arguments);
            _process.Start(Arg.Any<ProcessStartInfo>()).Returns(x => {
                startInfo = x[0] as ProcessStartInfo;
                return true;
            });

            _transcoder.Start(_transcodeJob);

            Assert.IsNotNull(startInfo);
            Assert.IsTrue(startInfo.CreateNoWindow);
            Assert.AreEqual(_ffmpegFileName, startInfo.FileName);
            Assert.AreEqual(arguments, startInfo.Arguments);
            Assert.IsTrue(startInfo.RedirectStandardOutput);
            Assert.IsTrue(startInfo.RedirectStandardError);
            Assert.IsFalse(startInfo.UseShellExecute);
        }

        [TestMethod]
        public void StopKillsProcess()
        {
            _transcoder.Start(_transcodeJob);
            _process.HasExited.Returns(false);

            _transcoder.Stop();

            _process.Received().Kill();
        }

        [TestMethod]
        public void StopDisposesProcess()
        {
            _transcoder.Start(_transcodeJob);
            _process.HasExited.Returns(false);

            _transcoder.Stop();

            _process.Received().Dispose();
        }

        [TestMethod]
        public void RaisesStatusChangedEvent()
        {
            TranscodeStatus status = null;

            _transcodeJob.SourceInfo.Duration = new TimeSpan(0, 1, 23, 43, 480);

            _transcoder.Start(_transcodeJob);
            _transcoder.StatusChanged += s => status = s;

            _process.ErrorDataReceived += Raise.Event<Action<string>>(
                "frame= 1439 fps=3.3 q=-0.0 size=  119516kB time=01:02:47.61 " +
                "bitrate=16315.1kbits/s speed=0.139x");

            Assert.IsNotNull(status);
            Assert.AreEqual(0.75, status.Percent);
            Assert.AreEqual(3.3, status.FramesPerSecond);
            Assert.AreEqual(122384384, status.Size);
            Assert.AreEqual(163179179, status.EstimatedTotalSize);
            Assert.AreEqual(0.139, status.Speed);
            Assert.AreEqual(new TimeSpan(0, 1, 2, 47, 610), status.Time);
            Assert.AreEqual(new TimeSpan(0, 2, 30, 35, 36), status.Eta);
        }

        [TestMethod]
        public void RaisesCompletedEvent()
        {
            bool completed = false;

            _transcoder.Start(_transcodeJob);
            _transcoder.Completed += () => completed = true;
            _process.ExitCode.Returns(0);

            _process.Exited += Raise.Event<Action>();

            Assert.IsTrue(completed);
        }

        [TestMethod]
        public void RaisesFailedEvent()
        {
            string expected = "transcode error";
            string actual = null;

            _transcoder.Start(_transcodeJob);
            _transcoder.Failed += e => actual = e;
            _process.ExitCode.Returns(1);

            _process.ErrorDataReceived += Raise.Event<Action<string>>(expected);
            _process.ErrorDataReceived += Raise.Event<Action<string>>("Conversion failed!");
            _process.Exited += Raise.Event<Action>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DisposesProcessOnExit()
        {
            _transcoder.Start(_transcodeJob);

            _process.Exited += Raise.Event<Action>();

            _process.Received().Dispose();
        }
    }
}
