using Tricycle.Models;

namespace Tricycle.Utilities
{
    public interface ITranscodeCalculator
    {
        /// <summary>
        /// Calculates crop parameters.
        /// </summary>
        /// <param name="sourceDimensions">The dimensions of the source.</param>
        /// <param name="autocropParameters">The detected crop parameters.</param>
        /// <param name="aspectRatio">The desired aspect ratio.</param>
        /// <param name="divisor">The number that height and width should be divisible by.</param>
        /// <returns>The calculated crop parameters.</returns>
        CropParameters CalculateCropParameters(Dimensions sourceDimensions,
                                               CropParameters autocropParameters,
                                               double aspectRatio,
                                               int divisor);
    }
}
