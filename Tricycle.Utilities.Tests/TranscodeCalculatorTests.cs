using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tricycle.Models;

namespace Tricycle.Utilities.Tests
{
    [TestClass]
    public class TranscodeCalculatorTests
    {
        [TestMethod]
        public void TestCalculateCropParameters()
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
    }
}
