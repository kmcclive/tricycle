using System;
namespace Tricycle.Media.Models
{
    public enum StreamType
    {
        Other,
        Audio,
        Video,
        Subtitle
    }

    public class StreamInfo
    {
        public int Index { get; set; }
        public StreamType StreamType { get; set; }
        public string FormatName { get; set; }
        public string Language { get; set; }
    }
}
