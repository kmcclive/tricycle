using System.Collections.Generic;
using System.Linq;
using Tricycle.Models;

namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class AudioConfig
    {
        public IDictionary<AudioFormat, AudioCodec> Codecs { get; set; }

        public AudioConfig Clone()
        {
            return new AudioConfig()
            {
                Codecs = Codecs?.ToDictionary(p => p.Key, p => p.Value?.Clone())
            };
        }
    }
}
