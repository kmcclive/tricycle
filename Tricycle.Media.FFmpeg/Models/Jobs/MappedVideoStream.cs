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
