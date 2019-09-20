using System;
using Tricycle.Models;

namespace Tricycle.Utilities
{
    public static class VideoUtility
    {
        public static bool SupportsHdr(VideoFormat format)
        {
            return format == VideoFormat.Hevc;
        }

        public static double GetAspectRatio(Dimensions dimensions)
        {
            return dimensions.Width / (double)dimensions.Height;
        }

        public static int GetWidth(int height, double aspectRatio)
        {
            return (int)Math.Round(height * aspectRatio);
        }

        public static int GetHeight(int width, double aspectRatio)
        {
            return (int)Math.Round(width / aspectRatio);
        }
    }
}
