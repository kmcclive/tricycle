using System.Collections.Generic;
using System.Linq;

namespace Tricycle.Models.Config
{
    public class VideoConfig
    {
        public IDictionary<VideoFormat, VideoCodec> Codecs { get; set; }
        public IDictionary<string, Dimensions> SizePresets { get; set; }
        public IDictionary<string, Dimensions> AspectRatioPresets { get; set; }
        public int SizeDivisor { get; set; }
        public SmartSwitchOption Deinterlace { get; set; } = SmartSwitchOption.Auto;

        public VideoConfig Clone()
        {
            return new VideoConfig()
            {
                Codecs = Codecs?.ToDictionary(p => p.Key, p => p.Value?.Clone()),
                SizePresets = SizePresets?.ToDictionary(p => p.Key, p => p.Value),
                AspectRatioPresets = AspectRatioPresets?.ToDictionary(p => p.Key, p => p.Value),
                SizeDivisor = SizeDivisor,
                Deinterlace = Deinterlace
            };
        }
    }
}
