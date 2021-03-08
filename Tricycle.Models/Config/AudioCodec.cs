using System.Collections.Generic;
using System.Linq;

namespace Tricycle.Models.Config
{
    public class AudioCodec
    {
        public string Tag { get; set; }
        public IList<AudioPreset> Presets { get; set; }

        public AudioCodec Clone()
        {
            return new AudioCodec
            {
                Tag = Tag,
                Presets = Presets?.Select(p => p?.Clone()).ToList()
            };
        }
    }
}
