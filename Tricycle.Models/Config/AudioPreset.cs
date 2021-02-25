namespace Tricycle.Models.Config
{
    public class AudioPreset
    {
        public AudioMixdown Mixdown { get; set; }
        public decimal Quality { get; set; }

        public AudioPreset Clone()
        {
            return new AudioPreset
            {
                Mixdown = Mixdown,
                Quality = Quality
            };
        }
    }
}
