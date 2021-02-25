namespace Tricycle.Models.Config
{
    public class VideoCodec
    {
        public Range<decimal> QualityRange { get; set; }
        public int QualitySteps { get; set; }
        public string Tag { get; set; }

        public VideoCodec Clone()
        {
            return new VideoCodec()
            {
                QualityRange = QualityRange,
                QualitySteps = QualitySteps,
                Tag = Tag
            };
        }
    }
}
