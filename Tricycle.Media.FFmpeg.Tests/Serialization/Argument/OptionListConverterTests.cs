using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Media.FFmpeg.Models.Jobs;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Tests.Serialization.Argument
{
    [TestClass]
    public class OptionListConverterTests
    {
        OptionListConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new OptionListConverter();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertThrowsExceptionWhenValueIsNotOptionList()
        {
            _converter.Convert("-x265-params", 0);
        }

        [TestMethod]
        public void ConvertIncludesOptionName()
        {
            var option = Option.FromName("hable");

            Assert.AreEqual("-tonemap hable",
                            _converter.Convert("-tonemap", new Option[] { option }));
        }

        [TestMethod]
        public void ConvertIncludesOptionValue()
        {
            var option = Option.FromValue("1");

            Assert.AreEqual("-setsar 1",
                            _converter.Convert("-setsar", new Option[] { option }));
        }

        [TestMethod]
        public void ConvertIncludesOptionNameAndValue()
        {
            var option = new Option("desat", "0");

            var filter = new Filter("tonemap")
            {
                Options = new Option[]
                {
                    new Option("desat", "0")
                }
            };

            Assert.AreEqual("-tonemap desat=0",
                            _converter.Convert("-tonemap", new Option[] { option }));
        }

        [TestMethod]
        public void ConvertDelimitsOptions()
        {
            var options = new Option[]
            {
                new Option("colorprim", "bt2020"),
                new Option("colormatrix", "bt2020nc"),
                new Option("transfer", "smpte2084")
            };

            Assert.AreEqual("-x265-params colorprim=bt2020:colormatrix=bt2020nc:transfer=smpte2084",
                            _converter.Convert("-x265-params", options));
        }
    }
}
