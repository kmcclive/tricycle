using Tricycle.Models;

namespace Tricycle.Utilities
{
    public static class VideoUtility
    {
        public static bool SupportsHdr(VideoFormat format)
        {
            return format == VideoFormat.Hevc;
        }
    }
}
