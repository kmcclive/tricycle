using System;
using System.Collections.Generic;
using System.Linq;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class SubtitleConfig
    {
        public IDictionary<SubtitleFormat, SubtitleCodec> Codecs { get; set; }

        public SubtitleConfig Clone()
        {
            return new SubtitleConfig()
            {
                Codecs = Codecs?.ToDictionary(p => p.Key, p => p.Value?.Clone())
            };
        }
    }
}
