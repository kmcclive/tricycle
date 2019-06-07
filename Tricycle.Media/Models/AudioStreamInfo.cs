using System;
namespace Tricycle.Media.Models
{
    public class AudioStreamInfo : StreamInfo
    {
        public int ChannelCount { get; set; }

        public AudioStreamInfo()
        {
            StreamType = StreamType.Audio;
        }
    }
}
