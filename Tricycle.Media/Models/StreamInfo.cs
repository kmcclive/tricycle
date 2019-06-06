using System;
namespace Tricycle.Media.Models
{
    public enum StreamType
    {
        Audio,
        Video,
        Subtitle
    }

    public class StreamInfo
    {
        public int Index { get; set; }
        public StreamType StreamType { get; set; }
        public string Name { get; set; }
    }
}
