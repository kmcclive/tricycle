using System;
using System.Collections.Generic;
using Tricycle.Media.FFmpeg.Serialization.Argument;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class Codec
    {
        [ArgumentOrder(0)]
        public string Name { get; set; }

        public Codec()
        {

        }

        public Codec(string name)
        {
            Name = name;
        }
    }
}
