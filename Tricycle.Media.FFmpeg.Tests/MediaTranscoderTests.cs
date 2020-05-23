using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;
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
        FFmpegJob _ffmpegJob;

        [TestInitialize]
        public void Setup()
        {
            _ffmpegFileName = "usr/sbin/ffmpeg";
            _process = Substitute.For<IProcess>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _transcoder = new MediaTranscoder(_ffmpegFileName,
                                              () => _process,
                                              _configManager,
                                              _argumentGenerator);       

            _argumentGenerator.When(x => x.GenerateArguments(Arg.Any<FFmpegJob>()))
                              .Do(x => _ffmpegJob = x[0] as FFmpegJob);

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
            _ffmpegJob = null;
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
        [ExpectedException(typeof(NotSupportedException))]
        public void StartThrowsForHdrWithAvcFormat()
        {
            _videoOutput.Format = VideoFormat.Avc;
            _videoOutput.DynamicRange = DynamicRange.High;

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
        public void StartAssignsMp4FormatOnJob()
        {
            _transcodeJob.Format = ContainerFormat.Mp4;

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual("mp4", _ffmpegJob?.Format);
        }

        [TestMethod]
        public void StartAssignsMkvFormatOnJob()
        {
            _transcodeJob.Format = ContainerFormat.Mkv;

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual("matroska", _ffmpegJob?.Format);
        }

        [TestMethod]
        public void StartAssignsMetadataOnJob()
        {
            _transcodeJob.Metadata = new Dictionary<string, string>()
            {
                { "title", "Test" }
            };

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual(_transcodeJob.Metadata, _ffmpegJob?.Metadata);
        }

        [TestMethod]
        public void StartDoesNotAssignMaxMuxingQueueSizeOnJobByDefault()
        {
            _transcoder.Start(_transcodeJob);

            Assert.IsNull(_ffmpegJob?.MaxMuxingQueueSize);
        }

        [TestMethod]
        public void StartAssignsMaxMuxingQueueSizeOnJobForDolbyTrueHd()
        {
            var index = 1;

            _transcodeJob.SourceInfo.Streams.Add(new AudioStreamInfo()
            {
                Index = index,
                Format = AudioFormat.DolbyTrueHd
            });
            _transcodeJob.Streams.Add(new OutputStream()
            {
                SourceStreamIndex = index
            });

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual(1024, _ffmpegJob?.MaxMuxingQueueSize);
        }

        [TestMethod]
        public void StartAssignsX264CodecOnJobVideoStream()
        {
            var preset = "fast";

            _videoOutput.Format = VideoFormat.Avc;
            _videoOutput.Quality = 20;
            _configManager.Config = new FFmpegConfig()
            {
                Video = new VideoConfig()
                {
                    Codecs = new Dictionary<VideoFormat, VideoCodec>()
                    {
                        { VideoFormat.Avc, new VideoCodec(preset) }
                    }
                }
            };

            _transcoder.Start(_transcodeJob);

            var codec = _ffmpegJob.Streams.FirstOrDefault()?.Codec as X26xCodec;

            Assert.IsNotNull(codec);
            Assert.AreEqual("libx264", codec.Name);
            Assert.AreEqual(preset, codec.Preset);
            Assert.AreEqual(_videoOutput.Quality, codec.Crf);
        }

        [TestMethod]
        public void StartAssignsX265CodecOnJobVideoStream()
        {
            var preset = "slow";

            _videoOutput.Format = VideoFormat.Hevc;
            _videoOutput.Quality = 18;
            _configManager.Config = new FFmpegConfig()
            {
                Video = new VideoConfig()
                {
                    Codecs = new Dictionary<VideoFormat, VideoCodec>()
                    {
                        { VideoFormat.Hevc, new VideoCodec(preset) }
                    }
                }
            };

            _transcoder.Start(_transcodeJob);

            var codec = _ffmpegJob.Streams.FirstOrDefault()?.Codec as X26xCodec;

            Assert.IsNotNull(codec);
            Assert.AreEqual("libx265", codec.Name);
            Assert.AreEqual(preset, codec.Preset);
            Assert.AreEqual(_videoOutput.Quality, codec.Crf);
        }

        [TestMethod]
        public void StartAssignsColorPrimariesOnJobVideoStreamForHdr()
        {
            _videoOutput.Format = VideoFormat.Hevc;
            _videoOutput.DynamicRange = DynamicRange.High;

            _transcoder.Start(_transcodeJob);

            var stream = _ffmpegJob.Streams.FirstOrDefault() as MappedVideoStream;

            Assert.IsNotNull(stream);
            Assert.AreEqual("bt2020", stream.ColorPrimaries);
        }

        [TestMethod]
        public void StartAssignsColorTransferOnJobVideoStreamForHdr()
        {
            _videoOutput.Format = VideoFormat.Hevc;
            _videoOutput.DynamicRange = DynamicRange.High;

            _transcoder.Start(_transcodeJob);

            var stream = _ffmpegJob.Streams.FirstOrDefault() as MappedVideoStream;

            Assert.IsNotNull(stream);
            Assert.AreEqual("smpte2084", stream.ColorTransfer);
        }

        [TestMethod]
        public void StartAssignsColorSpaceOnJobVideoStreamForHdr()
        {
            _videoOutput.Format = VideoFormat.Hevc;
            _videoOutput.DynamicRange = DynamicRange.High;

            _transcoder.Start(_transcodeJob);

            var stream = _ffmpegJob.Streams.FirstOrDefault() as MappedVideoStream;

            Assert.IsNotNull(stream);
            Assert.AreEqual("bt2020nc", stream.ColorSpace);
        }

        [TestMethod]
        public void StartAddsOptionsOnJobVideoCodecForCopyHdrMetadata()
        {
            _videoSource.MasterDisplayProperties = new MasterDisplayProperties()
            {
                Red = new Coordinate<int>(34000, 16000),
                Green = new Coordinate<int>(13250, 34500),
                Blue = new Coordinate<int>(7500, 3000),
                WhitePoint = new Coordinate<int>(15635, 16450),
                Luminance = new Range<int>(1, 10000000)
            };
            _videoSource.LightLevelProperties = new LightLevelProperties()
            {
                MaxCll = 1000,
                MaxFall = 400
            };
            _videoOutput.Format = VideoFormat.Hevc;
            _videoOutput.DynamicRange = DynamicRange.High;
            _videoOutput.CopyHdrMetadata = true;

            _transcoder.Start(_transcodeJob);

            var codec = _ffmpegJob.Streams.FirstOrDefault()?.Codec as X265Codec;

            Assert.IsNotNull(codec);
            Assert.AreEqual(2, codec.Options?.Count);

            var option = codec.Options.FirstOrDefault(o => o.Name == "master-display");

            Assert.IsNotNull(option);
            Assert.AreEqual("\"G(13250,34500)B(7500,3000)R(34000,16000)WP(15635,16450)L(10000000,1)\"", option.Value);

            option = codec.Options.FirstOrDefault(o => o.Name == "max-cll");

            Assert.IsNotNull(option);
            Assert.AreEqual("\"1000,400\"", option.Value);
        }

        [TestMethod]
        public void StartAddsPassthruStreamOnJob()
        {
            int index = 1;

            _transcodeJob.SourceInfo.Streams.Add(new StreamInfo()
            {
                Index = index
            });
            _transcodeJob.Streams.Add(new OutputStream()
            {
                SourceStreamIndex = index
            });

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual(2, _ffmpegJob.Streams?.Count);

            var stream = _ffmpegJob.Streams[1];

            Assert.AreEqual(0, stream.Input?.FileIndex);
            Assert.AreEqual(index, stream.Input?.StreamIndex);
            Assert.AreEqual("copy", stream.Codec?.Name);
        }

        [TestMethod]
        public void StartAddsAacAudioStreamOnJob()
        {
            int index = 1;
            var format = AudioFormat.Aac;
            string codec = "libfdk_aac";

            _transcodeJob.SourceInfo.Streams.Add(new AudioStreamInfo()
            {
                Index = index
            });
            _transcodeJob.Streams.Add(new AudioOutputStream()
            {
                SourceStreamIndex = index,
                Format = format,
                Mixdown = AudioMixdown.Stereo,
                Quality = 160
            });
            _configManager.Config = new FFmpegConfig()
            {
                Audio = new AudioConfig()
                {
                    Codecs = new Dictionary<AudioFormat, AudioCodec>()
                    {
                        { format, new AudioCodec(codec) }
                    }
                }
            };

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual(2, _ffmpegJob.Streams?.Count);

            var stream = _ffmpegJob.Streams[1] as MappedAudioStream;

            Assert.AreEqual(0, stream.Input?.FileIndex);
            Assert.AreEqual(index, stream.Input?.StreamIndex);
            Assert.AreEqual(codec, stream.Codec?.Name);
            Assert.AreEqual(2, stream.ChannelCount);
            Assert.AreEqual("160k", stream.Bitrate);
        }

        [TestMethod]
        public void StartAddsAc3AudioStreamOnJob()
        {
            int index = 2;
            var format = AudioFormat.Ac3;
            string codec = "ac3_fixed";

            _transcodeJob.SourceInfo.Streams.Add(new AudioStreamInfo()
            {
                Index = index
            });
            _transcodeJob.Streams.Add(new AudioOutputStream()
            {
                SourceStreamIndex = index,
                Format = format,
                Mixdown = AudioMixdown.Surround5dot1,
                Quality = 640
            });
            _configManager.Config = new FFmpegConfig()
            {
                Audio = new AudioConfig()
                {
                    Codecs = new Dictionary<AudioFormat, AudioCodec>()
                    {
                        { format, new AudioCodec(codec) }
                    }
                }
            };

            _transcoder.Start(_transcodeJob);

            Assert.AreEqual(2, _ffmpegJob.Streams?.Count);

            var stream = _ffmpegJob.Streams[1] as MappedAudioStream;

            Assert.AreEqual(0, stream.Input?.FileIndex);
            Assert.AreEqual(index, stream.Input?.StreamIndex);
            Assert.AreEqual(codec, stream.Codec?.Name);
            Assert.AreEqual(6, stream.ChannelCount);
            Assert.AreEqual("640k", stream.Bitrate);
        }

        [TestMethod]
        public void StartAssignsStreamMetadataOnJob()
        {
            _videoOutput.Metadata = new Dictionary<string, string>()
            {
                { "title", "Video" }
            };

            _transcoder.Start(_transcodeJob);

            var stream = _ffmpegJob.Streams.FirstOrDefault();

            Assert.AreEqual(_videoOutput.Metadata, stream.Metadata);
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

            _transcoder.Stop();

            _process.Received().Kill();
        }

        [TestMethod]
        public void StopWaitsForProcessToExit()
        {
            _transcoder.Start(_transcodeJob);

            _transcoder.Stop();

            _process.Received().WaitForExit(Arg.Any<int>());
        }

        [TestMethod]
        public void StopDisposesProcess()
        {
            _transcoder.Start(_transcodeJob);

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
        public void ParsesFFmpegStatusWithAdditonalParameters()
        {
            TranscodeStatus status = null;

            _transcodeJob.SourceInfo.Duration = new TimeSpan(1, 53, 23);

            _transcoder.Start(_transcodeJob);
            _transcoder.StatusChanged += s => status = s;

            _process.ErrorDataReceived += Raise.Event<Action<string>>(
                "frame=10697 fps= 32 q=25.0 size= 462080kB time=00:07:26.07 " +
                "bitrate=8485.9kbits/s dup=1 drop=0 speed=1.35x");

            Assert.IsNotNull(status);
            Assert.AreEqual(0.0656, status.Percent, 0.0001);
            Assert.AreEqual(32, status.FramesPerSecond);
            Assert.AreEqual(473169920, status.Size);
            Assert.AreEqual(7216300056, status.EstimatedTotalSize);
            Assert.AreEqual(1.35, status.Speed);
            Assert.AreEqual(new TimeSpan(0, 0, 7, 26, 70), status.Time);
            Assert.AreEqual(new TimeSpan(0, 1, 18, 28, 837), status.Eta);
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
