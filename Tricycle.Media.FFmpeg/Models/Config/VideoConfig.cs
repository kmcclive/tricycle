using System.Collections.Generic;
using System.Linq;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class VideoConfig
    {
        public IDictionary<VideoFormat, VideoCodec> Codecs { get; set; }
        public string CropDetectOptions { get; set; }
        public string DeinterlaceOptions { get; set; }
        public string DenoiseOptions { get; set; }
        public string TonemapOptions { get; set; }

        public VideoConfig Clone()
        {
            return new VideoConfig()
            {
                Codecs = Codecs?.ToDictionary(p => p.Key, p => p.Value?.Clone()),
                CropDetectOptions = CropDetectOptions,
                DeinterlaceOptions = DeinterlaceOptions,
                DenoiseOptions = DenoiseOptions,
                TonemapOptions = TonemapOptions,
            };
        }
    }
}
