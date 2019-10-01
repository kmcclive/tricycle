using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class MediaTranscoderTests
    {
        string _ffmpegFileName;
        IProcess _process;
        IFFmpegArgumentGenerator _argumentGenerator;
        MediaTranscoder _transcoder;

        [TestInitialize]
        public void Setup()
        {
            _ffmpegFileName = "usr/sbin/ffmpeg";
            _process = Substitute.For<IProcess>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _transcoder = new MediaTranscoder(_ffmpegFileName, () => _process, _argumentGenerator);
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
            _transcoder.Start(new TranscodeJob());

            _process.HasExited.Returns(false);

            _transcoder.Start(new TranscodeJob());
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
            _transcoder.Start(new TranscodeJob());

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
            _transcoder.Start(new TranscodeJob());

            _process.HasExited.Returns(true);
            
            Assert.IsFalse(_transcoder.IsRunning);
        }

        [TestMethod]
        public void IsRunningReturnsTrueForRunningJob()
        {
            _transcoder.Start(new TranscodeJob());

            _process.HasExited.Returns(false);

            Assert.IsTrue(_transcoder.IsRunning);
        }

        [TestMethod]
        public void StartCallsArgumentGenerator()
        {
            var job = new TranscodeJob();

            _transcoder.Start(job);

            _argumentGenerator.Received().GenerateArguments(job);
        }

        [TestMethod]
        public void StartCallsProcessStart()
        {
            _transcoder.Start(new TranscodeJob());

            _process.Received().Start(Arg.Any<ProcessStartInfo>());
        }

        [TestMethod]
        public void StartPassesCorrectProcessStartInfo()
        {
            string arguments = "transcode arguments";
            ProcessStartInfo startInfo = null;

            _argumentGenerator.GenerateArguments(Arg.Any<TranscodeJob>()).Returns(arguments);
            _process.Start(Arg.Any<ProcessStartInfo>()).Returns(x => {
                startInfo = x[0] as ProcessStartInfo;
                return true;
            });

            _transcoder.Start(new TranscodeJob());

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
            _transcoder.Start(new TranscodeJob());
            _process.HasExited.Returns(false);

            _transcoder.Stop();

            _process.Received().Kill();
        }

        [TestMethod]
        public void StopDisposesProcess()
        {
            _transcoder.Start(new TranscodeJob());
            _process.HasExited.Returns(false);

            _transcoder.Stop();

            _process.Received().Dispose();
        }

        [TestMethod]
        public void RaisesStatusChangedEvent()
        {
            TranscodeStatus status = null;

            _transcoder.Start(new TranscodeJob()
            {
                SourceInfo = new MediaInfo()
                {
                    Duration = new TimeSpan(0, 1, 23, 43, 480)
                }
            });
            _transcoder.StatusChanged += s => status = s;

            _process.ErrorDataReceived += Raise.Event<Action<string>>(
                "frame= 1439 fps=3.3 q=-0.0 size=  119516kB time=01:02:47.61 " +
                "bitrate=16315.1kbits/s speed=0.139x");

            Assert.IsNotNull(status);
            Assert.AreEqual(0.75, status.Percent);
            Assert.AreEqual(3.3, status.FramesPerSecond);
            Assert.AreEqual(119516000, status.Size);
            Assert.AreEqual(159354667, status.EstimatedTotalSize);
            Assert.AreEqual(0.139, status.Speed);
            Assert.AreEqual(new TimeSpan(0, 1, 2, 47, 610), status.Time);
            Assert.AreEqual(new TimeSpan(0, 2, 30, 35, 36), status.Eta);
        }

        [TestMethod]
        public void RaisesCompletedEvent()
        {
            bool completed = false;

            _transcoder.Start(new TranscodeJob());
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

            _transcoder.Start(new TranscodeJob());
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
            _transcoder.Start(new TranscodeJob());

            _process.Exited += Raise.Event<Action>();

            _process.Received().Dispose();
        }
    }
}
