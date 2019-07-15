namespace Tricycle.Models.Media
{
    public class AudioStreamInfo : StreamInfo
    {
        public int ChannelCount { get; set; }
        public string ProfileName { get; set; }

        public AudioStreamInfo()
        {
            StreamType = StreamType.Audio;
        }
    }
}
