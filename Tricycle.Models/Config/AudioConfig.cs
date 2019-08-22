using System.Collections.Generic;

namespace Tricycle.Models.Config
{
    public class AudioConfig
    {
        public IDictionary<AudioFormat, AudioCodec> Codecs { get; set; }
        public bool PassthruMatchingTracks { get; set; }
    }
}
