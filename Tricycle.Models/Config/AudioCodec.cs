using System.Collections.Generic;

namespace Tricycle.Models.Config
{
    public class AudioCodec
    {
        public AudioFormat Format { get; set; }
        public IList<AudioPreset> Presets { get; set; }
    }
}
