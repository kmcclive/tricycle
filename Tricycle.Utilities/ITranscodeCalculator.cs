using Tricycle.Models;

namespace Tricycle.Utilities
{
    public interface ITranscodeCalculator
    {
        /// <summary>
        /// Calculates crop parameters.
        /// </summary>
        /// <param name="sourceDimensions">The dimensions of the source.</param>
        /// <param name="storageDimensions">The storage dimensions of the source.</param>
        /// <param name="autocropParameters">The detected crop parameters.</param>
        /// <param name="aspectRatio">The desired aspect ratio if different than source.</param>
        /// <param name="divisor">The number that height and width should be divisible by.</param>
        /// <returns>The calculated crop parameters.</returns>
        CropParameters CalculateCropParameters(Dimensions sourceDimensions,
                                               Dimensions storageDimensions,
                                               CropParameters autocropParameters,
                                               double? aspectRatio,
                                               int divisor);

        /// <summary>
        /// Calculates dimensions to use for scaling that preserve aspect ratio.
        /// </summary>
        /// <param name="sourceDimensions">The dimensions of the source.</param>
        /// <param name="targetDimensions">The desired dimensions of the destination.</param>
        /// <param name="divisor">The number that height and width should be divisible by.</param>
        /// <returns>The calculated scaled dimensions.</returns>
        Dimensions CalculateScaledDimensions(Dimensions sourceDimensions, Dimensions targetDimensions, int divisor);
    }
}
