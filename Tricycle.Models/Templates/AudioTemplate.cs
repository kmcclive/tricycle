using System;
namespace Tricycle.Models.Templates
{
    public class AudioTemplate
    {
        public string Language { get; set; }
        public int RelativeIndex { get; set; }
        public AudioFormat Format { get; set; }
        public AudioMixdown Mixdown { get; set; }
    }
}
