using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics;
using Tricycle.Diagnostics.Models;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models.Jobs;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class PreviewImageGeneratorTests
    {
        PreviewImageGenerator _imageGenerator;
        string _ffmpegFileName;
        IProcessRunner _processRunner;
        IFFmpegArgumentGenerator _argumentGenerator;
        IConfigManager<FFmpegConfig> _configManager;
        IFileSystem _fileSystem;
        int _imageCount;
        IFile _fileService;
        IPath _pathService;
        TimeSpan _timeout;
        string _tempPath;
        TranscodeJob _transcodeJob;
        IList<FFmpegJob> _ffmpegJobs;

        [TestInitialize]
        public void Setup()
        {
            _ffmpegFileName = "/usr/sbin/ffmpeg";
            _processRunner = Substitute.For<IProcessRunner>();
            _argumentGenerator = Substitute.For<IFFmpegArgumentGenerator>();
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _fileSystem = Substitute.For<IFileSystem>();
            _fileService = Substitute.For<IFile>();
            _pathService = Substitute.For<IPath>();
            _imageCount = 3;
            _timeout = TimeSpan.FromMilliseconds(10);
            _imageGenerator = new PreviewImageGenerator(_ffmpegFileName,
                                                        _processRunner,
                                                        _argumentGenerator,
                                                        _configManager,
                                                        _fileSystem,
                                                        _imageCount,
                                                        _timeout);
            _tempPath = "/Users/fred/temp";
            _transcodeJob = new TranscodeJob()
            {
                SourceInfo = new MediaInfo()
                {
                    FileName = "source",
                    Duration = TimeSpan.FromHours(1),
                    Streams = new StreamInfo[]
                    {
                        new VideoStreamInfo()
                        {
                            Index = 0
                        }
                    }
                },
                OutputFileName = "destination",
                Streams = new OutputStream[]
                {
                    new VideoOutputStream()
                    {
                        SourceStreamIndex = 0
                    }
                }
            };
            _ffmpegJobs = new List<FFmpegJob>();

            _argumentGenerator.When(x => x.GenerateArguments(Arg.Any<FFmpegJob>()))
                              .Do(x => _ffmpegJobs.Add(x[0] as FFmpegJob));
            _fileSystem.File.Returns(_fileService);
            _fileSystem.Path.Returns(_pathService);
            _pathService.GetTempPath().Returns(_tempPath);
            _pathService.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(x => string.Join('/', x.Args()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GenerateThrowsExceptionWhenJobIsNull()
        {
            await _imageGenerator.Generate(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GenerateThrowsExceptionWhenJobSourceDurationIsInvalid()
        {
            _transcodeJob.SourceInfo.Duration = TimeSpan.Zero;

            await _imageGenerator.Generate(_transcodeJob);
        }

        [TestMethod]
        public async Task GenerateUsesCorrectImageCount()
        {
            await _imageGenerator.Generate(_transcodeJob);

            Assert.AreEqual(_imageCount, _ffmpegJobs.Count);
        }

        [TestMethod]
        public async Task GenerateUsesUniqueTempFileNames()
        {
            await _imageGenerator.Generate(_transcodeJob);

            foreach (var job in _ffmpegJobs)
            {
                Assert.IsTrue(Regex.IsMatch(job.OutputFileName, $"^{_tempPath}/[\\w-]+\\.png$"));
            }

            Assert.AreEqual(_ffmpegJobs.Count, _ffmpegJobs.Distinct().Count());
        }

        [TestMethod]
        public async Task GenerateUsesCorrectStartTimes()
        {
            await _imageGenerator.Generate(_transcodeJob);

            var expectedStartTimes = new TimeSpan[]
            {
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromMinutes(45)
            };

            for (int i = 0; i < _ffmpegJobs.Count; i++)
            {
                Assert.AreEqual(_ffmpegJobs[i].StartTime, expectedStartTimes[i]);
            }
        }

        [TestMethod]
        public async Task GenerateUsesCorrectFrameCounts()
        {
            await _imageGenerator.Generate(_transcodeJob);

            foreach (var job in _ffmpegJobs)
            {
                Assert.AreEqual(1, job.FrameCount);
            }
        }

        [TestMethod]
        public async Task GenerateCallsMapOnBaseClass()
        {
            await _imageGenerator.Generate(_transcodeJob);

            foreach (var job in _ffmpegJobs)
            {
                Assert.AreEqual(_transcodeJob.SourceInfo.FileName, job.InputFileName);
            }
        }

        [TestMethod]
        public async Task GenerateCallsProcessRunner()
        {
            var args = "generated arguments";

            _argumentGenerator.GenerateArguments(Arg.Any<FFmpegJob>()).Returns(args);

            await _imageGenerator.Generate(_transcodeJob);

            await _processRunner.Received(_imageCount).Run(_ffmpegFileName, args, _timeout);
        }

        [TestMethod]
        public async Task GenerateReturnsImagesForSuccessfulProcesses()
        {
            int i = 0;

            _fileService.Exists(Arg.Any<string>()).Returns(true);
            _processRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
                          .Returns(x => new ProcessResult { ExitCode = i++ == 0 ? 1 : 0 });

            var result = await _imageGenerator.Generate(_transcodeJob);

            Assert.AreEqual(_imageCount - 1, result?.Count);
        }

        [TestMethod]
        public async Task GenerateReturnsImagesForExistingFiles()
        {
            int i = 0;

            _fileService.Exists(Arg.Any<string>()).Returns(x => i++ == 0 ? false : true);
            _processRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
                          .Returns(new ProcessResult());

            var result = await _imageGenerator.Generate(_transcodeJob);

            Assert.AreEqual(_imageCount - 1, result?.Count);
        }

        [TestMethod]
        public async Task GenerateReturnsCorrectFileNames()
        {
            _fileService.Exists(Arg.Any<string>()).Returns(true);
            _processRunner.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
                          .Returns(new ProcessResult());

            var result = await _imageGenerator.Generate(_transcodeJob);

            Assert.AreEqual(_imageCount, result?.Count);

            for (int i = 0; i < _ffmpegJobs.Count; i++)
            {
                Assert.AreEqual(_ffmpegJobs[i].OutputFileName, result[i]);
            }
        }
    }
}
