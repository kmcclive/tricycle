namespace Tricycle.Models.Media
{
    public class SubtitleStreamInfo : StreamInfo
    {
        public SubtitleType SubtitleType { get; set; }
        public SubtitleFormat? Format { get; set; }

        public SubtitleStreamInfo()
        {
            StreamType = StreamType.Subtitle;
        }
    }
}
