using System;
using Tricycle.Models;

namespace Tricycle.Utilities
{
    public class TranscodeCalculator : ITranscodeCalculator
    {
        public CropParameters CalculateCropParameters(Dimensions sourceDimensions,
                                                      CropParameters autocropParameters,
                                                      double aspectRatio,
                                                      int divisor)
        {
            var start = autocropParameters?.Start ?? new Coordinate<int>(0, 0);
            int targetX = start.X;
            int actualX = targetX;
            int targetY = start.Y;
            int actualY = targetY;
            Dimensions size = autocropParameters?.Size ?? sourceDimensions;
            int targetHeight = size.Height;
            int actualHeight = GetClosestValue(targetHeight, divisor);

            if (actualHeight < targetHeight)
            {
                actualY += (int)Math.Ceiling((targetHeight - actualHeight) / 2d);
            }

            var targetWidth = (int)Math.Round(actualHeight * aspectRatio);
            int actualWidth = GetClosestValue(targetWidth, divisor);

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

        int GetClosestValue(int targetValue, int divisor)
        {
            int result = targetValue;

            if (targetValue % divisor != 0)
            {
                var factor = (int)Math.Round((double)targetValue / divisor);

                result = divisor * factor;
            }

            return result;
        }
    }
}
