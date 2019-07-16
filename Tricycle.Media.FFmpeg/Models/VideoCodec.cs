using System;
namespace Tricycle.Media.FFmpeg.Models
{
    public class VideoCodec
    {
        public string Preset { get; set; }

        public VideoCodec()
        {

        }

        public VideoCodec(string preset)
        {
            Preset = preset;
        }
    }
}
