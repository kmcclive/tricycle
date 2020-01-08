namespace Tricycle.Models.Media
{
    public class AudioStreamInfo : StreamInfo
    {
        public AudioFormat? Format { get; set; }
        public int ChannelCount { get; set; }
        public string ProfileName { get; set; }

        public AudioStreamInfo()
        {
            StreamType = StreamType.Audio;
        }
    }
}
