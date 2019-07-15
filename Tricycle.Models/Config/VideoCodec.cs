namespace Tricycle.Models.Config
{
    public class VideoCodec
    {
        public VideoFormat Format { get; set; }
        public Range<decimal> QualityRange { get; set; }
        public int QualitySteps { get; set; }
    }
}
