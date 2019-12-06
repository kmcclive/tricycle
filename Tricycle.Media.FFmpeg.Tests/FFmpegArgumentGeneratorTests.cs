using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tricycle.Diagnostics.Utilities;
using Tricycle.IO;
using Tricycle.Media.FFmpeg.Models.Config;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Tests
{
    [TestClass]
    public class FFmpegArgumentGeneratorTests
    {
        #region Fields

        IProcessUtility _processUtility;
        FFmpegConfig _config;
        IConfigManager<FFmpegConfig> _configManager;
        FFmpegArgumentGenerator _generator;

        #endregion

        #region Test Setup

        [TestInitialize]
        public void Setup()
        {
            _processUtility = Substitute.For<IProcessUtility>();
            _config = new FFmpegConfig();
            _configManager = Substitute.For<IConfigManager<FFmpegConfig>>();
            _configManager.Config = _config;
            _generator = new FFmpegArgumentGenerator(_processUtility, _configManager);

            _processUtility.EscapeFilePath(Arg.Any<string>()).Returns(x => $"\"{x[0]}\"");
        }

        #endregion

        #region Test Methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateArgumentsThrowsForNullArgument()
        {
            _generator.GenerateArguments(default(FFmpegJob));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateArgumentsThrowsForEmptyInputFileName()
        {
            var job = new FFmpegJob();

            _generator.GenerateArguments(job);
        }

        [TestMethod]
        public void GenerateArgumentsUsesArgumentName()
        {
            var job = new FFmpegJob()
            {
                InputFileName = "test",
                Format = "mp4"
            };
            string actual = _generator.GenerateArguments(job);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Contains("-f mp4"), "The result did not contain the argument name.");
        }

        [TestMethod]
        public void GenerateArgumentsUsesArgumentConverter()
        {
            var job = new FFmpegJob()
            {
                InputFileName = "/Users/fred/Movies/movie.mkv"
            };
            string actual = _generator.GenerateArguments(job);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Contains($"\"{job.InputFileName}\""), "The result did not contain the converted value.");
        }

        [TestMethod]
        public void GenerateArgumentsUsesPlaceholderForMissingOutput()
        {
            var job = new FFmpegJob()
            {
                InputFileName = "test"
            };
            string actual = _generator.GenerateArguments(job);

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.EndsWith($"-f null -", StringComparison.InvariantCulture),
                          "The result did not end with the output placeholder.");
        }

        [TestMethod]
        public void GenerateArgumentsUsesArgumentPriority()
        {
            var job = new FFmpegJob()
            {
                InputFileName = "/Users/fred/Movies/source.mkv",
                OutputFileName = "/Users/fred/Movies/destination.m4v",
                CanvasSize = new Dimensions(1920, 1080),
                Format = "mp4"
            };
            string expected =
                $"-canvas_size {job.CanvasSize} -i \"{job.InputFileName}\" -f {job.Format} \"{job.OutputFileName}\"";
            string actual = _generator.GenerateArguments(job);


            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
