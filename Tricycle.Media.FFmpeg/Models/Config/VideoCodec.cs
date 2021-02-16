using System;
namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class VideoCodec
    {
        public string Preset { get; set; }
        public string Tag { get; set; }

        public VideoCodec()
        {

        }

        public VideoCodec(string preset)
        {
            Preset = preset;
        }

        public VideoCodec Clone()
        {
            return new VideoCodec(Preset)
            {
                Tag = Tag
            };
        }
    }
}
