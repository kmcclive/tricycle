using System.Collections.Generic;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class VideoConfig
    {
        public IDictionary<VideoFormat, VideoCodec> Codecs { get; set; }
        public string CropDetectOptions { get; set; }
        public string DenoiseOptions { get; set; }
        public string TonemapOptions { get; set; }
    }
}
