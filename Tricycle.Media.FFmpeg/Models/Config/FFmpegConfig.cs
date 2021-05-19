using System;

namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class FFmpegConfig
    {
        public Version Version { get; set; }
        public VideoConfig Video { get; set; }
        public AudioConfig Audio { get; set; }
        public SubtitleConfig Subtitles { get; set; }

        public FFmpegConfig Clone()
        {
            return new FFmpegConfig()
            {
                Version = Version,
                Video = Video?.Clone(),
                Audio = Audio?.Clone(),
                Subtitles = Subtitles?.Clone()
            };
        }
    }
}
