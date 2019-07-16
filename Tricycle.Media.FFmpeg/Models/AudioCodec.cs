using System;
namespace Tricycle.Media.FFmpeg.Models
{
    public class AudioCodec
    {
        public string Name { get; set; }
        public string Options { get; set; }

        public AudioCodec()
        {

        }

        public AudioCodec(string name)
            : this (name, null)
        {

        }

        public AudioCodec(string name, string options)
        {
            Name = name;
            Options = options;
        }
    }
}
