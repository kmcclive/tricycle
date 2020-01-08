using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class FFmpegJobRunnerBaseTests
    {
        #region Nested Types

        class MockJobRunner : FFmpegJobRunnerBase
        {
            public FFmpegJob JobToMap { get; set; }
            public FFmpegConfig ConfigPassed { get; private set; }

            public MockJobRunner(IConfigManager<FFmpegConfig> configManager,
                                 IFFmpegArgumentGenerator argumentGenerator)
                : base(configManager, argumentGenerator)
            {

            }

            public new string GenerateArguments(TranscodeJob job)
            {
                return base.GenerateArguments(job);
            }

            public FFmpegJob CallMap(TranscodeJob job, FFmpegConfig config)
            {
                return Map(job, config);
            }

            protected override FFmpegJob Map(TranscodeJob job, FFmpegConfig config)
            {
                ConfigPassed = config;

                return JobToMap ?? base.Map(job, config);
            }
        }

        #endregion

        #region Fields

        MockJobRunner _jobRunner;
        IConfigManager<FFmpegConfig> _configManager;
        IFFmpegArgumentGenerator _argumentGenerator;
        VideoStreamInfo _videoSource;
        VideoOutputStream _videoOutput;
        TranscodeJob _transcodeJob;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _jobRunner = new MockJobRunner(_configManager, _argumentGenerator);

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

        #endregion

        #region Test Methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MapThrowsExceptionWhenJobIsNull()
        {
            _jobRunner.CallMap(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSourceInfoIsNull()
        {
            _transcodeJob.SourceInfo = null;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSourceFileNameIsNull()
        {
            _transcodeJob.SourceInfo.FileName = null;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSourceFileNameIsEmpty()
        {
            _transcodeJob.SourceInfo.FileName = string.Empty;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSourceStreamsIsNull()
        {
            _transcodeJob.SourceInfo.Streams = null;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSourceStreamsIsEmpty()
        {
            _transcodeJob.SourceInfo.Streams.Clear();

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenOutputFileNameIsNull()
        {
            _transcodeJob.OutputFileName = null;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenOutputFileNameIsEmpty()
        {
            _transcodeJob.OutputFileName = string.Empty;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenStreamsIsNull()
        {
            _transcodeJob.Streams = null;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenStreamsIsEmpty()
        {
            _transcodeJob.Streams.Clear();

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void MapThrowsExceptionWhenSourceDoesNotContainAVideoStream()
        {
            _transcodeJob.SourceInfo.Streams = new StreamInfo[]
            {
                new AudioStreamInfo()
            };

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenSubtitlesIndexIsNotFound()
        {
            _transcodeJob.Subtitles = new SubtitlesConfig()
            {
                SourceStreamIndex = 1
            };

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenStreamsContainsAnInvalidIndex()
        {
            _transcodeJob.Streams[0].SourceStreamIndex = 1;

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MapThrowsExceptionWhenStreamsContainsAStreamMismatch()
        {
            _transcodeJob.Streams[0] = new AudioOutputStream()
            {
                SourceStreamIndex = 0
            };

            _jobRunner.CallMap(_transcodeJob, null);
        }

        [TestMethod]
        public void MapAssignsHideBanner()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.IsTrue(ffmpegJob.HideBanner);
        }

        [TestMethod]
        public void MapAssignsOverwrite()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.IsTrue(ffmpegJob.Overwrite);
        }

        [TestMethod]
        public void MapAssignsInputFileName()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(_transcodeJob.SourceInfo.FileName, ffmpegJob.InputFileName);
        }

        [TestMethod]
        public void MapAssignsOutputFileName()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(_transcodeJob.OutputFileName, ffmpegJob.OutputFileName);
        }

        [TestMethod]
        public void MapAssignsForcedSubtitlesOnly()
        {
            var subtitleStream = new SubtitleStreamInfo()
            {
                Index = 1
            };

            _transcodeJob.SourceInfo.Streams.Add(subtitleStream);
            _transcodeJob.Subtitles = new SubtitlesConfig()
            {
                SourceStreamIndex = subtitleStream.Index,
                ForcedOnly = true
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(_transcodeJob.Subtitles.ForcedOnly, ffmpegJob.ForcedSubtitlesOnly);
        }

        [TestMethod]
        public void MapAssignsCanvasSize()
        {
            var subtitleStream = new SubtitleStreamInfo()
            {
                Index = 1
            };

            _transcodeJob.SourceInfo.Streams.Add(subtitleStream);
            _videoSource.Dimensions = new Dimensions(1920, 1080);
            _transcodeJob.Subtitles = new SubtitlesConfig()
            {
                SourceStreamIndex = subtitleStream.Index
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(_videoSource.Dimensions, ffmpegJob.CanvasSize);
        }

        [TestMethod]
        public void MapAddsVideoStream()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(1, ffmpegJob.Streams?.Count);
            Assert.IsInstanceOfType(ffmpegJob.Streams[0], typeof(MappedVideoStream));
        }

        [TestMethod]
        public void MapAssignsInputOnVideoStream()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);
            var videoStream = ffmpegJob.Streams.OfType<MappedVideoStream>().FirstOrDefault();

            if (videoStream == null)
            {
                Assert.Inconclusive("The video stream was not added to the job.");
            }

            Assert.AreEqual(0, videoStream.Input?.FileIndex);
            Assert.AreEqual(_videoOutput.SourceStreamIndex, videoStream.Input?.StreamIndex);
        }

        [TestMethod]
        public void MapDoesNotAssignCodecOnVideoStream()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);
            var videoStream = ffmpegJob.Streams.OfType<MappedVideoStream>().FirstOrDefault();

            if (videoStream == null)
            {
                Assert.Inconclusive("The video stream was not added to the job.");
            }

            Assert.IsNull(videoStream.Codec);
        }

        [TestMethod]
        public void MapAddsNoFiltersByDefault()
        {
            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(0, ffmpegJob.Filters?.Count ?? 0);
        }

        [TestMethod]
        public void MapAddsGraphicSubtitleFilters()
        {
            var subtitleStream = new SubtitleStreamInfo()
            {
                Index = 1,
                SubtitleType = SubtitleType.Graphic
            };

            _transcodeJob.SourceInfo.Streams.Add(subtitleStream);
            _transcodeJob.Subtitles = new SubtitlesConfig()
            {
                SourceStreamIndex = subtitleStream.Index
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(2, ffmpegJob.Filters?.Count);

            var scale2RefFilter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(scale2RefFilter);
            Assert.AreEqual("scale2ref", scale2RefFilter.Name);
            Assert.AreEqual(2, scale2RefFilter.Inputs?.Count);
            Assert.AreEqual(2, scale2RefFilter.OutputLabels?.Count);

            var streamInput = scale2RefFilter.Inputs[0] as StreamInput;

            Assert.IsNotNull(streamInput);
            Assert.AreEqual(0, streamInput.FileIndex);
            Assert.AreEqual(_transcodeJob.Subtitles.SourceStreamIndex, streamInput.StreamIndex);

            streamInput = scale2RefFilter.Inputs[1] as StreamInput;

            Assert.IsNotNull(streamInput);
            Assert.AreEqual(0, streamInput.FileIndex);
            Assert.AreEqual(_videoOutput.SourceStreamIndex, streamInput.StreamIndex);

            var overlayFilter = ffmpegJob.Filters[1] as Filter;

            Assert.IsNotNull(overlayFilter);
            Assert.AreEqual("overlay", overlayFilter.Name);
            Assert.IsTrue(overlayFilter.ChainToPrevious);
            Assert.AreEqual(2, overlayFilter.Inputs?.Count);

            var labeledInput = overlayFilter.Inputs[0] as LabeledInput;

            Assert.IsNotNull(labeledInput);
            Assert.AreEqual(labeledInput.Label, scale2RefFilter.OutputLabels[1]);

            labeledInput = overlayFilter.Inputs[1] as LabeledInput;

            Assert.IsNotNull(labeledInput);
            Assert.AreEqual(labeledInput.Label, scale2RefFilter.OutputLabels[0]);
        }

        [TestMethod]
        public void MapAddsTextSubtitleFilter()
        {
            var subtitleStream = new SubtitleStreamInfo()
            {
                Index = 2,
                SubtitleType = SubtitleType.Text
            };

            _transcodeJob.SourceInfo.Streams.Add(new SubtitleStreamInfo()
            {
                Index = 1
            });
            _transcodeJob.SourceInfo.Streams.Add(subtitleStream);
            _transcodeJob.SourceInfo.Streams.Add(new SubtitleStreamInfo()
            {
                Index = 3
            });
            _transcodeJob.Subtitles = new SubtitlesConfig()
            {
                SourceStreamIndex = subtitleStream.Index
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(1, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("subtitles", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual($"\"{_transcodeJob.SourceInfo.FileName}\"", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.AreEqual("si", option.Name);
            Assert.AreEqual("1", option.Value);
        }

        [TestMethod]
        public void MapAddsCropFilter()
        {
            _videoSource.Dimensions = new Dimensions(1920, 1080);
            _videoOutput.CropParameters = new CropParameters()
            {
                Size = new Dimensions(1920, 800),
                Start = new Coordinate<int>(0, 140)
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(2, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("crop", filter.Name);
            Assert.AreEqual(4, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.CropParameters.Size.Width.ToString(), option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.CropParameters.Size.Height.ToString(), option.Value);

            option = filter.Options[2];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.CropParameters.Start.X.ToString(), option.Value);

            option = filter.Options[3];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.CropParameters.Start.Y.ToString(), option.Value);
        }

        [TestMethod]
        public void MapAddsSetSarFilterForCrop()
        {
            _videoSource.Dimensions = new Dimensions(1920, 1080);
            _videoOutput.CropParameters = new CropParameters()
            {
                Size = new Dimensions(1920, 800),
                Start = new Coordinate<int>(0, 140)
            };

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(2, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[1] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("setsar", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("1", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("1", option.Value);
        }

        [TestMethod]
        public void MapAddsScaleFilter()
        {
            _videoSource.Dimensions = new Dimensions(1920, 1080);
            _videoOutput.ScaledDimensions = new Dimensions(1280, 720);

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(2, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("scale", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.ScaledDimensions.Value.Width.ToString(), option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual(_videoOutput.ScaledDimensions.Value.Height.ToString(), option.Value);
        }

        [TestMethod]
        public void MapAddsSetSarFilterForScale()
        {
            _videoSource.Dimensions = new Dimensions(1920, 1080);
            _videoOutput.ScaledDimensions = new Dimensions(1280, 720);

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(2, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[1] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("setsar", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("1", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("1", option.Value);
        }

        [TestMethod]
        public void MapAddsDenoiseFilter()
        {
            _videoOutput.Denoise = true;

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(1, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("hqdn3d", filter.Name);
            Assert.AreEqual(4, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("4", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("4", option.Value);

            option = filter.Options[2];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("3", option.Value);

            option = filter.Options[3];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("3", option.Value);
        }

        [TestMethod]
        public void MapUsesDenoiseOptionsFromConfig()
        {
            var config = new FFmpegConfig()
            {
                Video = new VideoConfig()
                {
                    DenoiseOptions = "nlmeans"
                }
            };

            _videoOutput.Denoise = true;

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, config);

            Assert.AreEqual(1, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as CustomFilter;

            Assert.IsNotNull(filter);
            Assert.AreEqual(config.Video.DenoiseOptions, filter.Data);
        }

        [TestMethod]
        public void MapAddsTonemapFilters()
        {
            _videoOutput.Tonemap = true;

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, null);

            Assert.AreEqual(6, ffmpegJob.Filters?.Count);

            var filter = ffmpegJob.Filters[0] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("zscale", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            var option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.AreEqual("t", option.Name);
            Assert.AreEqual("linear", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.AreEqual("npl", option.Name);
            Assert.AreEqual("100", option.Value);

            filter = ffmpegJob.Filters[1] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("format", filter.Name);
            Assert.AreEqual(1, filter.Options?.Count);

            option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("gbrpf32le", option.Value);

            filter = ffmpegJob.Filters[2] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("zscale", filter.Name);
            Assert.AreEqual(1, filter.Options?.Count);

            option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.AreEqual("p", option.Name);
            Assert.AreEqual("bt709", option.Value);

            filter = ffmpegJob.Filters[3] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("tonemap", filter.Name);
            Assert.AreEqual(2, filter.Options?.Count);

            option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("hable", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.AreEqual("desat", option.Name);
            Assert.AreEqual("0", option.Value);

            filter = ffmpegJob.Filters[4] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("zscale", filter.Name);
            Assert.AreEqual(3, filter.Options?.Count);

            option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.AreEqual("t", option.Name);
            Assert.AreEqual("bt709", option.Value);

            option = filter.Options[1];

            Assert.IsNotNull(option);
            Assert.AreEqual("m", option.Name);
            Assert.AreEqual("bt709", option.Value);

            option = filter.Options[2];

            Assert.IsNotNull(option);
            Assert.AreEqual("r", option.Name);
            Assert.AreEqual("tv", option.Value);

            filter = ffmpegJob.Filters[5] as Filter;

            Assert.IsNotNull(filter);
            Assert.AreEqual("format", filter.Name);
            Assert.AreEqual(1, filter.Options?.Count);

            option = filter.Options[0];

            Assert.IsNotNull(option);
            Assert.IsNull(option.Name);
            Assert.AreEqual("yuv420p", option.Value);
        }

        [TestMethod]
        public void MapUsesTonemapOptionsFromConfig()
        {
            var config = new FFmpegConfig()
            {
                Video = new VideoConfig()
                {
                    TonemapOptions = "reinhard"
                }
            };

            _videoOutput.Tonemap = true;

            var ffmpegJob = _jobRunner.CallMap(_transcodeJob, config);
            var filter = ffmpegJob.Filters.OfType<CustomFilter>().FirstOrDefault();

            Assert.IsNotNull(filter);
            Assert.AreEqual($"tonemap={config.Video.TonemapOptions}", filter.Data);
        }

        [TestMethod]
        public void GenerateArgumentsCallsMapWithConfig()
        {
            _configManager.Config = new FFmpegConfig();

            _jobRunner.GenerateArguments(_transcodeJob);

            Assert.AreEqual(_configManager.Config, _jobRunner.ConfigPassed);
        }

        [TestMethod]
        public void GenerateArgumentsCallsArgumentGeneratorWithJob()
        {
            _jobRunner.JobToMap = new FFmpegJob();

            _jobRunner.GenerateArguments(_transcodeJob);

            _argumentGenerator.Received().GenerateArguments(_jobRunner.JobToMap);
        }

        #endregion
    }
}
