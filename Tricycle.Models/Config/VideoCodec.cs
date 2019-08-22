namespace Tricycle.Models.Config
{
    public class VideoCodec
    {
        public Range<decimal> QualityRange { get; set; }
        public int QualitySteps { get; set; }
    }
}
