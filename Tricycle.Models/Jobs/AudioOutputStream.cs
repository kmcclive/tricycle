namespace Tricycle.Models.Jobs
{
    public class AudioOutputStream : TranscodedOutputStream<AudioFormat>
    {
        public AudioMixdown? Mixdown { get; set; }
        public decimal? Quality { get; set; }
    }
}
