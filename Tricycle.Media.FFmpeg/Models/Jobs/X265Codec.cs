using System;
using System.Collections.Generic;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class X265Codec : X26xCodec
    {
        [Argument("-x265-params")]
        [ArgumentConverter(typeof(OptionListConverter))]
        [ArgumentOrder(1)]
        public IList<Option> Options { get; set; }

        public X265Codec()
        {

        }

        public X265Codec(string name)
            : base(name)
        {

        }
    }
}
