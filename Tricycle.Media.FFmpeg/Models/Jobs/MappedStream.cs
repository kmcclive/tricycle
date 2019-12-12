﻿using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedStream
    {
        [ArgumentIgnore]
        public StreamType StreamType { get; set; }

        [ArgumentIgnore]
        public StreamInput Input { get; set; }

        [Argument("-c")]
        public Codec Codec { get; set; }

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
