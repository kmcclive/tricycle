﻿using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedAudioStream : MappedStream
    {
        [Argument("-ac")]
        public int? ChannelCount { get; set; }

        public MappedAudioStream()
            : base(StreamType.Audio)
        {

        }

        public MappedAudioStream(StreamInput input)
            : base(StreamType.Audio, input)
        {

        }
    }
}
