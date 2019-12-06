using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedStream
    {
        public StreamType StreamType { get; set; }
        public StreamInput Input { get; set; }

        [Argument("-c")]
        public string Codec { get; set; }

        [Argument("-b")]
        public string Bitrate { get; set; }

        public MappedStream()
        {

        }

        public MappedStream(StreamType streamType)
        {
            StreamType = streamType;
        }

        public MappedStream(StreamType streamType, StreamInput input)
            : this(streamType)
        {
            Input = input;
        }
    }
}
