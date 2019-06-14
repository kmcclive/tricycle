using System;
using System.Collections.Generic;
using Tricycle.Media.Models;

namespace Tricycle.Models
{
    public class VideoConfig
    {
        public IList<VideoCodec> Codecs { get; set; }
        public IDictionary<string, Dimensions> SizePresets { get; set; }
        public IDictionary<string, Dimensions> AspectRatioPresets { get; set; }
    }
}
