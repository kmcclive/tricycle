using System;
using Tricycle.Models.Media;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class MappedVideoStream : MappedStream
    {
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
