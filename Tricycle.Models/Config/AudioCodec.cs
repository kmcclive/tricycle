using System.Collections.Generic;

namespace Tricycle.Models.Config
{
    public class AudioCodec
    {
        public string Tag { get; set; }
        public IList<AudioPreset> Presets { get; set; }
    }
}
