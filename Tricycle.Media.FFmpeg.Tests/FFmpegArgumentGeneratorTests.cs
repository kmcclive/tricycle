using System;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests;

[TestClass]
public class FFmpegArgumentGeneratorTests
{
    #region Fields

    FFmpegArgumentGenerator _generator;
    IArgumentPropertyReflector _reflector;
    IArgumentConverter _mockConverter;

    #endregion

    #region Test Setup

    [TestInitialize]
    public void Setup()
    {
        _reflector = Substitute.For<IArgumentPropertyReflector>();
        _generator = new FFmpegArgumentGenerator(_reflector);
        _mockConverter = Substitute.For<IArgumentConverter>();

        _mockConverter.Convert(Arg.Any<string>(), Arg.Any<object>()).Returns(x => $"{x[0]} {x[1]}");
    }

    #endregion

    #region Test Methods

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GenerateArgumentsThrowsForNullArgument()
    {
        _generator.GenerateArguments(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GenerateArgumentsThrowsForEmptyInputFileName()
    {
        var job = new FFmpegJob();

        _generator.GenerateArguments(job);
    }

    [TestMethod]
    public void GenerateArgumentsCallsReflector()
    {
        var job = new FFmpegJob()
        {
            InputFileName = "test"
        };

        _generator.GenerateArguments(job);

        _reflector.Received().Reflect(job);
    }

    [TestMethod]
    public void GenerateArgumentsUsesArgumentName()
    {
        var job = new FFmpegJob()
        {
            InputFileName = "test"
        };

        _reflector.Reflect(job).Returns(new ArgumentProperty[]
        {
            new ArgumentProperty()
            {
                ArgumentName = "-f",
                Converter = _mockConverter
            }
        });

        string actual = _generator.GenerateArguments(job);

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Contains("-f"), "The result did not contain the argument name.");
    }

    [TestMethod]
    public void GenerateArgumentsUsesArgumentConverter()
    {
        var job = new FFmpegJob()
        {
            InputFileName = "/Users/fred/Movies/movie.mkv"
        };

        _reflector.Reflect(job).Returns(new ArgumentProperty[]
        {
            new ArgumentProperty()
            {
                Value = job.InputFileName,
                Converter = _mockConverter
            }
        });

        string actual = _generator.GenerateArguments(job);

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Contains(job.InputFileName), "The result did not contain the converted value.");
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
    public void GenerateArgumentsDelimitsArguments()
    {
        var job = new FFmpegJob()
        {
            InputFileName = "/Users/fred/Movies/source.mkv",
            OutputFileName = "/Users/fred/Movies/destination.m4v",
            Format = "mp4"
        };

        _reflector.Reflect(job).Returns(new ArgumentProperty[]
        {
            new ArgumentProperty()
            {
                ArgumentName = "-i",
                Value = job.InputFileName,
                Converter = _mockConverter
            },
            new ArgumentProperty()
            {
                ArgumentName = "-f",
                Value = job.Format,
                Converter = _mockConverter
            },
            new ArgumentProperty()
            {
                ArgumentName = "-o",
                Value = job.OutputFileName,
                Converter = _mockConverter
            }
        });

        string expected = $"-i {job.InputFileName} -f {job.Format} -o {job.OutputFileName}";
        string actual = _generator.GenerateArguments(job);

        Assert.AreEqual(expected, actual);
    }

    #endregion
}
