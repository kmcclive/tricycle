using System;
namespace Tricycle.Media.Models
{
    public enum DynamicRange
    {
        Standard,
        High
    }

    public class VideoStreamInfo : StreamInfo
    {
        public Dimensions Dimensions { get; set; }
        public DynamicRange DynamicRange { get; set; }

        public VideoStreamInfo()
        {
            StreamType = StreamType.Video;
        }
    }
}
