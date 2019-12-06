using System;
using System.Collections.Generic;

namespace Tricycle.Media.FFmpeg.Models.Jobs
{
    public class Codec
    {
        public string Name { get; set; }
        public IDictionary<string, string> Options { get; set; }

        public Codec()
        {

        }

        public Codec(string name)
        {
            Name = name;
        }
    }
}
