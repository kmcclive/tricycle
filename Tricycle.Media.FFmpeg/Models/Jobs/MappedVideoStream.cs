using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedVideoStream : MappedStream
    {
        [Argument("-color_primaries")]
        public string ColorPrimaries { get; set; }

        [Argument("-color_trc")]
        public string ColorTransfer { get; set; }

        [Argument("-colorspace")]
        public string ColorSpace { get; set; }

        [Argument("-master_display_red_x")]
        public decimal? MasterDisplayRedX { get; set; }

        [Argument("-master_display_red_y")]
        public decimal? MasterDisplayRedY { get; set; }

        [Argument("-master_display_green_x")]
        public decimal? MasterDisplayGreenX { get; set; }

        [Argument("-master_display_green_y")]
        public decimal? MasterDisplayGreenY { get; set; }

        [Argument("-master_display_blue_x")]
        public decimal? MasterDisplayBlueX { get; set; }

        [Argument("-master_display_blue_y")]
        public decimal? MasterDisplayBlueY { get; set; }

        [Argument("-master_display_white_x")]
        public decimal? MasterDisplayWhiteX { get; set; }

        [Argument("-master_display_white_y")]
        public decimal? MasterDisplayWhiteY { get; set; }

        [Argument("-master_display_min_lum")]
        public decimal? MasterDisplayMinLuminance { get; set; }

        [Argument("-master_display_max_lum")]
        public decimal? MasterDisplayMaxLuminance { get; set; }

        [Argument("-max_cll")]
        public decimal? MaxCll { get; set; }

        [Argument("-max_fall")]
        public decimal? MaxFall { get; set; }

        public MappedVideoStream()
            : base(StreamType.Video)
        {

        }

        public MappedVideoStream(StreamInput input)
            : base(StreamType.Video, input)
        {

        }
    }
}
