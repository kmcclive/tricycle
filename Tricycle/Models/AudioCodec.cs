using System;
using System.Collections.Generic;

namespace Tricycle.Models
{
    public enum AudioFormat
    {
        Aac,
        HeAAc,
        Ac3
    }

    public class AudioCodec
    {
        public AudioFormat Format { get; set; }
        public IList<AudioPreset> Presets { get; set; }
    }
}
