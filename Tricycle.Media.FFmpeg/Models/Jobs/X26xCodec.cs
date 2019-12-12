using System;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class X26xCodec : Codec
    {
        [Argument("-preset")]
        [ArgumentOrder(1)]
        public string Preset { get; set; }

        [Argument("-crf")]
        [ArgumentOrder(1)]
        public decimal Crf { get; set; }

        public X26xCodec()
        {

        }

        public X26xCodec(string name)
            : base(name)
        {

        }
    }
}
