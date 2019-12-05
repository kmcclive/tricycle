using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedStream
    {
        public StreamInput Input { get; set; }
        public StreamType StreamType { get; set; }

        [Argument("-c")]
        public string Codec { get; set; }

        [Argument("-b")]
        public string Bitrate { get; set; }
    }
}
