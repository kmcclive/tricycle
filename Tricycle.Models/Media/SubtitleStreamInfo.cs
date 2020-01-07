namespace Tricycle.Models.Media
{
    public class SubtitleStreamInfo : StreamInfo
    {
        public SubtitleType SubtitleType { get; set; }

        public SubtitleStreamInfo()
        {
            StreamType = StreamType.Subtitle;
        }
    }
}
