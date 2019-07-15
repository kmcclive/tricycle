namespace Tricycle.Models.Media
{
    public class VideoStreamInfo : StreamInfo
    {
        public Dimensions Dimensions { get; set; }
        public DynamicRange DynamicRange { get; set; }
        public int BitDepth { get; set; }
        public MasterDisplayProperties MasterDisplayProperties { get; set; }
        public LightLevelProperties LightLevelProperties { get; set; }

        public VideoStreamInfo()
        {
            StreamType = StreamType.Video;
        }
    }
}
