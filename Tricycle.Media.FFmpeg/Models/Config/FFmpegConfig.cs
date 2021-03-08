namespace Tricycle.Media.FFmpeg.Models.Config
{
    public class FFmpegConfig
    {
        public VideoConfig Video { get; set; }
        public AudioConfig Audio { get; set; }

        public FFmpegConfig Clone()
        {
            return new FFmpegConfig()
            {
                Video = Video?.Clone(),
                Audio = Audio?.Clone()
            };
        }
    }
}
