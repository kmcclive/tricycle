using System.Collections.Generic;

namespace Tricycle.Models.Config
{
    public class VideoConfig
    {
        public IDictionary<VideoFormat, VideoCodec> Codecs { get; set; }
        public IDictionary<string, Dimensions> SizePresets { get; set; }
        public IDictionary<string, Dimensions> AspectRatioPresets { get; set; }
        public int SizeDivisor { get; set; }
    }
}
