using System;
using Tricycle.Models;

namespace Tricycle.Utilities
{
    public class TranscodeCalculator : ITranscodeCalculator
    {
        enum EstimationMethod
        {
            Round,
            Floor
        }

        public CropParameters CalculateCropParameters(Dimensions sourceDimensions,
                                                      Dimensions storageDimensions,
                                                      CropParameters autocropParameters,
                                                      double? aspectRatio,
                                                      int divisor)
        {
            var sampleAspectRatio = VideoUtility.GetSampleAspectRatio(sourceDimensions, storageDimensions);
            var start = autocropParameters?.Start ?? new Coordinate<int>(0, 0);
            int targetX = (int)Math.Round(start.X * sampleAspectRatio);
            int actualX = targetX;
            int targetY = start.Y;
            int actualY = targetY;
            Dimensions size = autocropParameters?.Size ?? sourceDimensions;
            int targetHeight = size.Height;
            var heightMethod = targetHeight < sourceDimensions.Height ? EstimationMethod.Floor : EstimationMethod.Round;
            int actualHeight = GetClosestValue(targetHeight, divisor, heightMethod);

            if (actualHeight < targetHeight)
            {
                actualY += (int)Math.Ceiling((targetHeight - actualHeight) / 2d);
            }

            int targetWidth = size.Width;

            if (aspectRatio.HasValue)
            {
                targetWidth = VideoUtility.GetWidth(actualHeight, aspectRatio.Value);
            }

            var widthMethod = !aspectRatio.HasValue && size.Width < sourceDimensions.Width
                ? EstimationMethod.Floor
                : EstimationMethod.Round;
            int actualWidth = GetClosestValue(targetWidth, divisor, widthMethod);

            if (aspectRatio.HasValue)
            {
                actualWidth = (int)Math.Round(actualWidth / sampleAspectRatio);
            }

            if (actualWidth > size.Width)
            {
                actualWidth = GetClosestValue(targetWidth, divisor, EstimationMethod.Floor);

                if (aspectRatio.HasValue)
                {
                    actualWidth = (int)Math.Round(actualWidth / sampleAspectRatio);
                }
            }

            if (actualWidth < size.Width)
            {
                actualX += (int)Math.Floor((size.Width - actualWidth) / 2d);
            }

            return new CropParameters()
            {
                Start = new Coordinate<int>(actualX, actualY),
                Size = new Dimensions(actualWidth, actualHeight)
            };
        }

        public Dimensions CalculateScaledDimensions(Dimensions sourceDimensions, Dimensions targetDimensions, int divisor)
        {
            double sourceAspectRatio = VideoUtility.GetAspectRatio(sourceDimensions);
            double targetAspectRatio = VideoUtility.GetAspectRatio(targetDimensions);
            int targetHeight, targetWidth, actualHeight, actualWidth;

            if (targetAspectRatio < sourceAspectRatio)
            {
                actualWidth = GetClosestValue(targetDimensions.Width, divisor, EstimationMethod.Round);
                targetHeight = VideoUtility.GetHeight(actualWidth, sourceAspectRatio);
                actualHeight = GetClosestValue(targetHeight, divisor, EstimationMethod.Round);
            }
            else
            {
                actualHeight = GetClosestValue(targetDimensions.Height, divisor, EstimationMethod.Round);
                targetWidth = VideoUtility.GetWidth(actualHeight, sourceAspectRatio);
                actualWidth = GetClosestValue(targetWidth, divisor, EstimationMethod.Round);
            }

            return new Dimensions(actualWidth, actualHeight);
        }

        int GetClosestValue(int targetValue, int divisor, EstimationMethod method)
        {
            int result = targetValue;

            if (targetValue % divisor != 0)
            {
                var factor = (double)targetValue / divisor;
                int roundedFactor;

                if (method == EstimationMethod.Floor)
                {
                    roundedFactor = (int)Math.Floor(factor);
                }
                else
                {
                    roundedFactor = (int)Math.Round(factor);
                }

                result = divisor * roundedFactor;
            }

            return result;
        }
    }
}
