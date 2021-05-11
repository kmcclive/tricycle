using System;
namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class SubtitleCodec
    {
        public string Name { get; set; }

        public SubtitleCodec()
        {
        }

        public SubtitleCodec(string name)
        {
            Name = name;
        }

        public SubtitleCodec Clone()
        {
            return new SubtitleCodec(Name);
        }
    }
}
