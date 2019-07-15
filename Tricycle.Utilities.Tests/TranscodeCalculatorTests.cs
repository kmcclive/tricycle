using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Models;

namespace Tricycle.Utilities.Tests
{
    [TestClass]
    public class TranscodeCalculatorTests
    {
        [TestMethod]
        public void CalculatesCropParametersFor16x9WithHorizontalBars()
        {
            var calculator = new TranscodeCalculator();
            var sourceDimensions = new Dimensions(1920, 1080);
            var autocropParameters = new CropParameters()
            {
                Size = new Dimensions(1920, 804),
                Start = new Coordinate<int>(0, 138)
            };

            CropParameters result =
                calculator.CalculateCropParameters(sourceDimensions, autocropParameters, 16 / 9d, 8);

            Assert.IsNotNull(result);
            Assert.AreEqual(new Dimensions(1424, 800), result.Size);
            Assert.AreEqual(new Coordinate<int>(248, 140), result.Start);
        }

        [TestMethod]
        public void CalculatesCropParametersFor21x9WithHorizontalBars()
        {
            var calculator = new TranscodeCalculator();
            var sourceDimensions = new Dimensions(3840, 2160);
            var autocropParameters = new CropParameters()
            {
                Size = new Dimensions(3840, 1632),
                Start = new Coordinate<int>(0, 264)
            };

            var result =
                calculator.CalculateCropParameters(sourceDimensions, autocropParameters, 21 / 9d, 16);

            Assert.IsNotNull(result);
            Assert.AreEqual(new Dimensions(3808, 1632), result.Size);
            Assert.AreEqual(new Coordinate<int>(16, 264), result.Start);
        }

        [TestMethod]
        public void CalculatesCropParametersFor4x3WithNoBars()
        {
            var calculator = new TranscodeCalculator();
            var sourceDimensions = new Dimensions(853, 480);

            CropParameters result = calculator.CalculateCropParameters(sourceDimensions, null, 4 / 3d, 16);

            Assert.IsNotNull(result);
            Assert.AreEqual(new Dimensions(640, 480), result.Size);
            Assert.AreEqual(new Coordinate<int>(106, 0), result.Start);
        }

        [TestMethod]
        public void CalculatesCropParametersWhenNoChangesAreRequired()
        {
            var calculator = new TranscodeCalculator();
            var sourceDimensions = new Dimensions(1920, 1080);
            var autocropParameters = new CropParameters()
            {
                Size = new Dimensions(1440, 1080),
                Start = new Coordinate<int>(0, 240)
            };

            CropParameters result =
                calculator.CalculateCropParameters(sourceDimensions, autocropParameters, 4 / 3d, 8);

            Assert.IsNotNull(result);
            Assert.AreEqual(autocropParameters.Size, result.Size);
            Assert.AreEqual(autocropParameters.Start, result.Start);
        }
    }
}
