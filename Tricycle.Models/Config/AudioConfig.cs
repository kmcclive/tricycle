using System.Collections.Generic;
using System.Linq;

namespace Tricycle.Models.Config
{
    public class AudioConfig
    {
        public IDictionary<AudioFormat, AudioCodec> Codecs { get; set; }
        public bool PassthruMatchingTracks { get; set; }

        public AudioConfig Clone()
        {
            return new AudioConfig()
            {
                Codecs = Codecs?.ToDictionary(p => p.Key, p => p.Value?.Clone()),
                PassthruMatchingTracks = PassthruMatchingTracks
            };
        }
    }
}
