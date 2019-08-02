using System.Collections.Generic;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models
{
    public class VideoConfig
    {
        public IDictionary<VideoFormat, VideoCodec> Codecs { get; set; }
        public string TonemapOptions { get; set; }
    }
}
