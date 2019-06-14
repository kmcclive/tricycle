using System;
namespace Tricycle.Models
{
    public enum AudioMixdown
    {
        Mono,
        Stereo,
        Surround5dot1,
    }

    public class AudioPreset
    {
        public AudioMixdown Mixdown { get; set; }
        public decimal Quality { get; set; }
    }
}
